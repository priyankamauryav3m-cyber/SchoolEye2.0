/*
    V3M_FIN_GetStudentCollectionReport  (View Student Receipt -> Collection Report)

    One row per receipt, with the receipt's current status, so the report can show
    what was collected and what happened to it afterwards.

    Section
      'PERIOD'  : receipts made in @FromDate..@ToDate (the day's / period's collection)
      'EARLIER' : receipts made BEFORE @FromDate that were cancelled / refunded / bounced in the period
                  (shown in their own section; they change this period's cash, not its collection)

    Status (EventDate = when the status happened)
      ACTIVE    : Valid = 1, no cheque bounce
      BOUNCED   : V3M_FIN_StudentChequeBounce row (IsValid = 1) - the receipt itself stays Valid = 1;
                  money never received                                        EventDate = bounce date
      ADVANCE   : Valid = 0, CancelSettlement = 1 - cancelled, amount kept as student advance
                                                                              EventDate = CancelDate
      REFUND    : Valid = 0, CancelSettlement = 2 - cancelled, amount refunded to parent
                                                                              EventDate = CancelDate
      CANCELLED : Valid = 0, no settlement recorded (cancelled before the settlement option existed)
                                                                              EventDate = CancelDate
      EDITED    : CancelType = 2 - replaced by a revised receipt (OriginalReceiptId) through Edit Receipt.
                  Not collection: the revised receipt carries the money.

    Filters : same as the search of View Student Receipt (Status / Receipt No. / Student / Cheque No. / Cancel Type
              are not used: the report always shows every status).
    Caller  : StudentFeeReceiptService.GetStudentCollectionReport
    Safety  : read-only, no data is changed.
    Rollback: DROP PROCEDURE dbo.V3M_FIN_GetStudentCollectionReport;
*/
SET NOCOUNT ON;
GO

--EXEC dbo.V3M_FIN_GetStudentCollectionReport @GroupCode = '01', @BranchCode = '01', @FromDate = '2026-11-02', @ToDate = '2026-11-02';
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetStudentCollectionReport
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @SessionId      BIGINT       = 0,
    @GroupClassCode VARCHAR(20)  = '',
    @ClassCode      VARCHAR(20)  = '',
    @PaymentMode    VARCHAR(10)  = '',
    @ReceiptMode    INT          = -1,     -- IsAdmissionReceipt: -1 = all
    @FromDate       DATE,
    @ToDate         DATE,
    @SocietyId      INT          = 0,
    @BankAccountId  INT          = 0,
    @FeeReceivedBy  VARCHAR(50)  = ''      -- CreatedBy
AS
BEGIN
    SET NOCOUNT ON;

    SET @GroupClassCode = NULLIF(NULLIF(LTRIM(RTRIM(@GroupClassCode)), ''), '0');
    SET @ClassCode      = NULLIF(NULLIF(LTRIM(RTRIM(@ClassCode)), ''), '0');
    SET @PaymentMode    = NULLIF(NULLIF(LTRIM(RTRIM(@PaymentMode)), ''), '0');
    SET @FeeReceivedBy  = NULLIF(NULLIF(LTRIM(RTRIM(@FeeReceivedBy)), ''), '0');

    ;WITH R AS
    (
        SELECT rm.ReceiptId,
               rm.ReceiptNo,
               vs.StudentNo,
               vs.StudentName,
               ISNULL(NULLIF(vs.ClassSection, ''), vs.ClassName) AS ClassSection,
               ISNULL(NULLIF(LTRIM(RTRIM(rm.PaymentMode)), ''), 'Other') AS PaymentMode,
               rm.ChequeNo,
               rm.ChequeBank,
               rm.ChequeDate,
               rm.CreatedDate AS ReceiptDate,
               ISNULL(rm.PaidAmount, 0) AS PaidAmount,
               rm.CreatedBy,
               rm.CancelDate,
               rm.CancelBy,
               rm.Remark,
               orig.ReceiptNo AS OriginalReceiptNo,
               cb.BounceDate,
               CASE WHEN ISNULL(rm.CancelType, 0) = 2           THEN 'EDITED'
                    WHEN cb.BounceDate IS NOT NULL              THEN 'BOUNCED'
                    WHEN rm.Valid = 1                           THEN 'ACTIVE'
                    WHEN rm.CancelSettlement = 1                THEN 'ADVANCE'
                    WHEN rm.CancelSettlement = 2                THEN 'REFUND'
                    ELSE 'CANCELLED' END AS [Status],
               CASE WHEN ISNULL(rm.CancelType, 0) = 2           THEN rm.CancelDate
                    WHEN cb.BounceDate IS NOT NULL              THEN cb.BounceDate
                    WHEN rm.Valid = 1                           THEN NULL
                    ELSE rm.CancelDate END AS EventDate
        FROM dbo.V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
        INNER JOIN dbo.vwStudentDetails vs WITH (NOLOCK)
                ON vs.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
               AND vs.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
               AND vs.SessionId  = rm.SessionId
               AND vs.StudentId  = rm.StudentId
        OUTER APPLY
        (
            SELECT TOP 1 b.CreatedDate AS BounceDate
            FROM dbo.V3M_FIN_StudentChequeBounce b WITH (NOLOCK)
            WHERE b.ReceiptId = rm.ReceiptId AND b.IsValid = 1
              AND b.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
              AND b.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
            ORDER BY b.CreatedDate
        ) cb
        OUTER APPLY
        (
            SELECT TOP 1 o.ReceiptNo
            FROM dbo.V3M_FIN_StudentReceiptMaster o WITH (NOLOCK)
            WHERE o.ReceiptId = rm.OriginalReceiptId
        ) orig
        WHERE rm.GroupCode  = @GroupCode
          AND rm.BranchCode = @BranchCode
          AND CAST(rm.CreatedDate AS DATE) <= @ToDate
          AND (@SessionId     = 0     OR rm.SessionId = @SessionId)
          AND (@BankAccountId = 0     OR rm.BankAccountId = @BankAccountId)
          AND (@FeeReceivedBy IS NULL OR rm.CreatedBy = @FeeReceivedBy)
          AND (@SocietyId     = 0     OR rm.SocietyId = @SocietyId)
          AND (@ClassCode     IS NULL OR rm.ClassCode = @ClassCode)
          AND (@PaymentMode   IS NULL OR rm.PaymentMode = @PaymentMode)
          AND (@ReceiptMode   = -1    OR rm.IsAdmissionReceipt = @ReceiptMode)
          AND (@GroupClassCode IS NULL
               OR EXISTS (SELECT 1
                          FROM dbo.V3M_DetClassGroup cg WITH (NOLOCK)
                          WHERE cg.IsValid = 1
                            AND cg.GroupCode  = @GroupCode
                            AND cg.BranchCode = @BranchCode
                            AND cg.CGID       = @GroupClassCode
                            AND cg.ClassCode COLLATE DATABASE_DEFAULT = vs.ClassCode COLLATE DATABASE_DEFAULT))
          -- made in the period, or made earlier and its status changed in the period
          AND (CAST(rm.CreatedDate AS DATE) >= @FromDate
               OR CAST(rm.CancelDate AS DATE) BETWEEN @FromDate AND @ToDate
               OR cb.BounceDate IS NOT NULL)
    )
    SELECT CASE WHEN CAST(R.ReceiptDate AS DATE) >= @FromDate THEN 'PERIOD' ELSE 'EARLIER' END AS Section,
           R.ReceiptId, R.ReceiptNo, R.StudentNo, R.StudentName, R.ClassSection, R.PaymentMode,
           R.ChequeNo, R.ChequeBank, R.ChequeDate, R.ReceiptDate, R.PaidAmount, R.CreatedBy,
           R.[Status], R.EventDate, R.CancelBy, R.Remark, R.OriginalReceiptNo
    FROM R
    WHERE CAST(R.ReceiptDate AS DATE) >= @FromDate
       OR (R.[Status] IN ('BOUNCED', 'ADVANCE', 'REFUND', 'CANCELLED')
           AND CAST(R.EventDate AS DATE) BETWEEN @FromDate AND @ToDate)
    ORDER BY Section DESC, R.ReceiptDate, R.ReceiptId
    OPTION (RECOMPILE);
END
GO
