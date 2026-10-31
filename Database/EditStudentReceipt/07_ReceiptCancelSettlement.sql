/*
    Cancel Receipt — settlement (Keep as Advance / Refund to Parent) + refund OTP
    ----------------------------------------------------------------
    View Student Receipt -> Cancel (Cancel Type = Other):
      1 = Keep as Advance : dues come back on the invoices, the receipt amount stays with the
                            school as student advance (V3M_Fin_StudentBalance, SB ledger row)
                            and is adjusted in the next fee collection.
      2 = Refund to Parent: dues come back on the invoices, the amount is returned to the parent.
                            Needs an OTP sent to the parent (StudentParent.FatherEMail; SMS mobile
                            option shown, SMS not sent yet). Journal voucher of the receipt is made invalid.
    Cheque Bounce: no settlement (money was never received).

    Objects : V3M_FIN_StudentReceiptMaster.CancelSettlement (new NULLable column)
              V3M_FIN_ReceiptRefundOtp (new table, OTP stored as SHA-256 hash only)
              V3M_FIN_GetReceiptRefundContacts, V3M_FIN_SaveReceiptRefundOtp,
              V3M_FIN_GetReceiptRefundDetails (refund receipt PDF / confirmation mail) (new procedures)
              V3M_FIN_CancelStudentReceipt (@SettleMode / @RefundOtpHash) -> 06 script
    Index   : IX on (ReceiptId, IsUsed) — the OTP is always looked up by receipt.
    Rollback: EditStudentReceipt_Rollback.sql
*/

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.V3M_FIN_StudentReceiptMaster', 'CancelSettlement') IS NULL
    ALTER TABLE dbo.V3M_FIN_StudentReceiptMaster ADD CancelSettlement TINYINT NULL;   -- 1 = kept as advance, 2 = refunded
GO

IF OBJECT_ID('dbo.V3M_FIN_ReceiptRefundOtp', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.V3M_FIN_ReceiptRefundOtp
    (
        RefundOtpId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_V3M_FIN_ReceiptRefundOtp PRIMARY KEY,
        GroupCode   VARCHAR(10)  NOT NULL,
        BranchCode  VARCHAR(10)  NOT NULL,
        ReceiptId   BIGINT       NOT NULL,
        Channel     VARCHAR(10)  NOT NULL,                 -- Email / Mobile
        SentTo      VARCHAR(100) NOT NULL,                 -- masked address only
        OtpHash     VARCHAR(64)  NOT NULL,                 -- SHA-256 (hex) of ReceiptId:OTP, never the OTP itself
        ExpiryUtc   DATETIME2(0) NOT NULL,
        Attempts    TINYINT      NOT NULL CONSTRAINT DF_V3M_FIN_ReceiptRefundOtp_Attempts DEFAULT 0,
        IsUsed      BIT          NOT NULL CONSTRAINT DF_V3M_FIN_ReceiptRefundOtp_IsUsed DEFAULT 0,
        CreatedBy   VARCHAR(50)  NULL,
        CreatedDate DATETIME     NOT NULL CONSTRAINT DF_V3M_FIN_ReceiptRefundOtp_CreatedDate DEFAULT GETDATE(),
        UsedDate    DATETIME     NULL
    );

    CREATE INDEX IX_V3M_FIN_ReceiptRefundOtp_Receipt ON dbo.V3M_FIN_ReceiptRefundOtp (ReceiptId, IsUsed) INCLUDE (ExpiryUtc, Attempts);
END
GO

/*
    V3M_FIN_GetReceiptRefundContacts
    Parent email / SMS mobile of the student of an active receipt (StudentParent).
    The API masks them before they reach the screen.
*/
--EXEC dbo.V3M_FIN_GetReceiptRefundContacts '01', '01', 6;
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetReceiptRefundContacts
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ReceiptId  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
           rm.ReceiptId,
           rm.ReceiptNo,
           rm.PaidAmount,
           rm.Valid,
           LTRIM(RTRIM(ISNULL(sp.FatherEMail, ''))) AS FatherEMail,
           LTRIM(RTRIM(ISNULL(sp.SMSMobileNo, ''))) AS SMSMobileNo
    FROM dbo.V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
    LEFT JOIN dbo.StudentParent sp WITH (NOLOCK)
        ON sp.StudentId = rm.StudentId
       AND sp.GroupCode COLLATE DATABASE_DEFAULT = rm.GroupCode COLLATE DATABASE_DEFAULT
       AND sp.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
    WHERE rm.GroupCode = @GroupCode AND rm.BranchCode = @BranchCode AND rm.ReceiptId = @ReceiptId
    ORDER BY sp.ParentID DESC;
END
GO

/*
    V3M_FIN_SaveReceiptRefundOtp
    @Result : 1 saved, -1 receipt not found / already cancelled, -2 an OTP was sent less than 30 seconds ago
    Older unused OTPs of the receipt are closed, only the latest one can be used.
*/
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_SaveReceiptRefundOtp
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ReceiptId  BIGINT,
    @Channel    VARCHAR(10),
    @SentTo     VARCHAR(100),
    @OtpHash    VARCHAR(64),
    @ExpiryUtc  DATETIME2(0),
    @CreatedBy  VARCHAR(50),
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Result = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster
                   WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId AND Valid = 1)
    BEGIN SET @Result = -1; RETURN; END

    IF EXISTS (SELECT 1 FROM dbo.V3M_FIN_ReceiptRefundOtp
               WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode
                 AND CreatedDate > DATEADD(SECOND, -30, GETDATE()))
    BEGIN SET @Result = -2; RETURN; END

    BEGIN TRANSACTION;

    UPDATE dbo.V3M_FIN_ReceiptRefundOtp
    SET IsUsed = 1, UsedDate = GETDATE()
    WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND IsUsed = 0;

    INSERT INTO dbo.V3M_FIN_ReceiptRefundOtp (GroupCode, BranchCode, ReceiptId, Channel, SentTo, OtpHash, ExpiryUtc, CreatedBy)
    VALUES (@GroupCode, @BranchCode, @ReceiptId, @Channel, @SentTo, @OtpHash, @ExpiryUtc, @CreatedBy);

    COMMIT TRANSACTION;
    SET @Result = 1;
END
GO

/*
    V3M_FIN_GetReceiptRefundDetails
    Refund receipt (PDF / confirmation mail) of a receipt cancelled with "Refund to Parent".
      Result 1 : header (FatherEMail is used by the server for the mail only)
      Result 2 : fee heads refunded, months joined, amount per head
    Nothing is returned when the receipt was not refunded (CancelSettlement <> 2).
*/
--EXEC dbo.V3M_FIN_GetReceiptRefundDetails '01', '01', 7;
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetReceiptRefundDetails
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ReceiptId  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster WITH (NOLOCK)
                   WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId
                     AND Valid = 0 AND CancelSettlement = 2)
        RETURN;

    SELECT rm.ReceiptId,
           rm.ReceiptNo,
           rm.CreatedDate AS ReceiptDate,
           rm.PaymentMode,
           rm.ChequeNo,
           rm.PaidAmount  AS RefundAmount,
           rm.CancelDate  AS RefundDate,
           rm.CancelBy    AS RefundedBy,
           rm.Remark,
           vs.StudentNo,
           vs.StudentName,
           vs.FatherName,
           ISNULL(NULLIF(vs.ClassSection, ''), vs.ClassName) AS ClassSection,
           vs.SessionName,
           LTRIM(RTRIM(ISNULL(sp.FatherEMail, ''))) AS FatherEMail,
           otp.SentTo AS OtpSentTo
    FROM dbo.V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
    INNER JOIN dbo.vwStudentDetails vs WITH (NOLOCK)
        ON vs.GroupCode COLLATE DATABASE_DEFAULT = rm.GroupCode COLLATE DATABASE_DEFAULT
       AND vs.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
       AND vs.SessionId = rm.SessionId
       AND vs.StudentId = rm.StudentId
    OUTER APPLY (
        SELECT TOP 1 LTRIM(RTRIM(ISNULL(p.FatherEMail, ''))) AS FatherEMail
        FROM dbo.StudentParent p WITH (NOLOCK)
        WHERE p.StudentId = rm.StudentId
          AND p.GroupCode COLLATE DATABASE_DEFAULT = rm.GroupCode COLLATE DATABASE_DEFAULT
          AND p.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
        ORDER BY p.ParentID DESC
    ) sp
    OUTER APPLY (
        SELECT TOP 1 o.SentTo
        FROM dbo.V3M_FIN_ReceiptRefundOtp o WITH (NOLOCK)
        WHERE o.ReceiptId = rm.ReceiptId AND o.GroupCode = rm.GroupCode AND o.BranchCode = rm.BranchCode
          AND o.IsUsed = 1 AND o.UsedDate IS NOT NULL
        ORDER BY o.RefundOtpId DESC
    ) otp
    WHERE rm.GroupCode = @GroupCode AND rm.BranchCode = @BranchCode AND rm.ReceiptId = @ReceiptId;

    -- receipt detail rows are Valid = 0 after the cancel: all rows of the receipt are the refunded amount
    SELECT ISNULL(fh.FeeHeadName, 'Fee') AS FeeHeadName,
           STRING_AGG(NULLIF(LEFT(md.[MonthName], 3), ''), ', ') WITHIN GROUP (ORDER BY ISNULL(md.DisplayOrder, rd.MonthNo)) AS Months,
           SUM(ISNULL(rd.PaidAmount, 0)) AS Amount
    FROM dbo.V3M_FIN_StudentReceiptDetail rd WITH (NOLOCK)
    LEFT JOIN dbo.V3M_FIN_MstFeeHead fh WITH (NOLOCK)
        ON fh.FeeHeadId = rd.FeeHeadId AND fh.GroupCode = rd.GroupCode AND fh.BranchCode = rd.BranchCode
    LEFT JOIN dbo.V3M_FIN_Mst_MonthDisplayOrder md WITH (NOLOCK)
        ON md.MonthNo = rd.MonthNo AND md.GroupCode = rd.GroupCode AND md.BranchCode = rd.BranchCode
    WHERE rd.ReceiptId = @ReceiptId AND rd.GroupCode = @GroupCode AND rd.BranchCode = @BranchCode
    GROUP BY fh.FeeHeadName, fh.DisplayOrder
    ORDER BY ISNULL(fh.DisplayOrder, 0);
END
GO
