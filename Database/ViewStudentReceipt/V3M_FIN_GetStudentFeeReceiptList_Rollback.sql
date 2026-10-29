/*
    V3M_FIN_GetStudentFeeReceiptList — rollback
    ----------------------------------------------------------------
    Restores the procedure exactly as it was before
    V3M_FIN_GetStudentFeeReceiptList_AddPeriodName.sql (no PeriodName column).
*/

SET NOCOUNT ON;
GO

--EXEC dbo.V3M_FIN_GetStudentFeeReceiptList @GroupCode = '01', @BranchCode = '01', @SessionId = 1043, @FromDate = '2026-10-01', @ToDate = '2026-10-31';
CREATE OR ALTER PROCEDURE [dbo].[V3M_FIN_GetStudentFeeReceiptList]
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @SessionId      BIGINT       = 0,
    @GroupClassCode VARCHAR(20)  = '',     -- CGID (class group)
    @ClassCode      VARCHAR(20)  = '',
    @PaymentMode    VARCHAR(10)  = '',
    @ReceiptMode    INT          = -1,     -- IsAdmissionReceipt: -1 = sab
    @ReceiptNo      VARCHAR(50)  = '',
    @StudentNo      VARCHAR(50)  = '',     -- ControlNo
    @FromDate       DATE,
    @ToDate         DATE,
    @Status         INT          = -1,     -- IsActive: -1 = sab
    @FirstName      VARCHAR(100) = '',
    @SocietyId      INT          = 0,
    @ChequeNo       VARCHAR(50)  = '',
    @BankAccountId  INT          = 0,
    @FeeReceivedBy  VARCHAR(50)  = '',     -- CreatedBy
    @CancelType     VARCHAR(10)  = '-1'
AS
BEGIN
    SET NOCOUNT ON;

    SET @GroupClassCode = NULLIF(NULLIF(LTRIM(RTRIM(@GroupClassCode)), ''), '0');
    SET @ClassCode      = NULLIF(NULLIF(LTRIM(RTRIM(@ClassCode)), ''), '0');
    SET @PaymentMode    = NULLIF(NULLIF(LTRIM(RTRIM(@PaymentMode)), ''), '0');
    SET @ReceiptNo      = NULLIF(LTRIM(RTRIM(@ReceiptNo)), '');
    SET @StudentNo      = NULLIF(NULLIF(LTRIM(RTRIM(@StudentNo)), ''), '0');
    SET @FirstName      = NULLIF(LTRIM(RTRIM(@FirstName)), '');
    SET @ChequeNo       = NULLIF(NULLIF(LTRIM(RTRIM(@ChequeNo)), ''), '0');
    SET @FeeReceivedBy  = NULLIF(NULLIF(LTRIM(RTRIM(@FeeReceivedBy)), ''), '0');
    SET @CancelType     = NULLIF(NULLIF(LTRIM(RTRIM(@CancelType)), ''), '-1');

    SELECT rm.ReceiptId,
           rm.ReceiptNo,
           vs.StudentId,
           vs.StudentNo,
           vs.ControlNo,
           vs.StudentName,
           vs.ClassName,
           vs.ClassSection,
           rm.PreviousBalance,
           rm.TotalAmount,
           rm.PaidAmount,
           rm.Concession,
           rm.Balance,
           rm.LateFee,
           rm.ChequeDate,
           rm.ChequeBank,
           rm.PaymentMode,
           rm.ChequeNo,
           rm.Valid,
           rm.CreatedDate AS ReceiptDate,
           rm.CreatedBy,
           ISNULL(rm.TotalAmount, 0) + ISNULL(rm.PreviousBalance, 0) + ISNULL(rm.LateFee, 0) AS TotalFeeAmount,
           ISNULL(rm.IsActive, 1) AS IsActive,
           con.ConcessionName,
           con.Type AS ConcessionType,
           rm.Remark
    FROM V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
    INNER JOIN vwStudentDetails vs WITH (NOLOCK)
            ON vs.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
           AND vs.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
           AND vs.SessionId  = rm.SessionId
           AND vs.StudentId  = rm.StudentId
    OUTER APPLY
    (
        SELECT TOP 1 mc.Concession AS ConcessionName, mc.Type
        FROM V3M_FIN_DetConcessionMapToStudent dt WITH (NOLOCK)
        INNER JOIN V3M_FIN_MstConcession mc WITH (NOLOCK) ON mc.ConcessionId = dt.ConcessionId
        WHERE dt.GroupCode  COLLATE DATABASE_DEFAULT = vs.GroupCode  COLLATE DATABASE_DEFAULT
          AND dt.BranchCode COLLATE DATABASE_DEFAULT = vs.BranchCode COLLATE DATABASE_DEFAULT
          AND dt.SessionId = vs.SessionId
          AND dt.StudentId = vs.StudentId
          AND dt.Status = 1
    ) con
    WHERE rm.GroupCode  = @GroupCode
      AND rm.BranchCode = @BranchCode
      AND CAST(rm.CreatedDate AS DATE) BETWEEN @FromDate AND @ToDate
      AND (@SessionId     = 0     OR rm.SessionId = @SessionId)
      AND (@BankAccountId = 0     OR rm.BankAccountId = @BankAccountId)
      AND (@FeeReceivedBy IS NULL OR rm.CreatedBy = @FeeReceivedBy)
      AND (@SocietyId     = 0     OR rm.SocietyId = @SocietyId)
      AND (@ClassCode     IS NULL OR rm.ClassCode = @ClassCode)
      AND (@PaymentMode   IS NULL OR rm.PaymentMode = @PaymentMode)
      AND (@StudentNo     IS NULL OR vs.ControlNo = @StudentNo)
      AND (@Status        = -1    OR rm.IsActive = @Status)
      AND (@FirstName     IS NULL OR vs.StudentName LIKE '%' + @FirstName + '%')
      AND (@ChequeNo      IS NULL OR rm.ChequeNo = @ChequeNo)
      AND (@ReceiptMode   = -1    OR rm.IsAdmissionReceipt = @ReceiptMode)
      AND (@ReceiptNo     IS NULL OR rm.ReceiptNo = @ReceiptNo)
      AND (@CancelType    IS NULL OR rm.CancelType = @CancelType)
      AND (@GroupClassCode IS NULL
           OR EXISTS (SELECT 1
                      FROM V3M_DetClassGroup cg WITH (NOLOCK)
                      WHERE cg.IsValid = 1
                        AND cg.GroupCode  = @GroupCode
                        AND cg.BranchCode = @BranchCode
                        AND cg.CGID       = @GroupClassCode
                        AND cg.ClassCode COLLATE DATABASE_DEFAULT = vs.ClassCode COLLATE DATABASE_DEFAULT))
    ORDER BY rm.ReceiptId DESC
    OPTION (RECOMPILE);
END


GO
