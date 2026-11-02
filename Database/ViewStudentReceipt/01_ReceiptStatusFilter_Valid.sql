/*
    View Student Receipt -> Status filter (Active / Cancelled)

    Problem : a cancelled receipt gets Valid = 0, but IsActive stays 1 (V3M_FIN_CancelStudentReceipt and
              V3M_FIN_GenerateReceiptChqBounceChallan do not touch IsActive). Both procedures below filtered
              @Status on rm.IsActive, so "Cancelled" returned nothing and "Active" also returned cancelled receipts.
    Change  : @Status now filters on rm.Valid (1 = active, 0 = cancelled, -1 = all) - the same column the grid
              shows (Done / Not Done) and Fin_GetOnlyChqCombineReport already uses. Nothing else is changed.
    Procs   : V3M_FIN_GetStudentFeeReceiptList   (search grid)
              V3M_FIN_PrintViewStudentReceipts   (Cheque Report -> Print)
    Safety  : read-only procedures, no data is changed.
    Rollback: 01_ReceiptStatusFilter_Valid_Rollback.sql
*/
--EXEC dbo.V3M_FIN_GetStudentFeeReceiptList @GroupCode = '01', @BranchCode = '01', @SessionId = 1043, @FromDate = '2026-10-01', @ToDate = '2026-10-31';
CREATE   PROCEDURE [dbo].[V3M_FIN_GetStudentFeeReceiptList]
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
           rm.Remark,
           per.PeriodName
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
    OUTER APPLY
    (
        SELECT STRING_AGG(LEFT(mo.MonthName, 3), ',') WITHIN GROUP (ORDER BY mo.DisplayOrder) AS PeriodName
        FROM V3M_FIN_Mst_MonthDisplayOrder mo WITH (NOLOCK)
        WHERE mo.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
          AND mo.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
          AND mo.IsValid = 1
          AND mo.MonthNo IN (SELECT DISTINCT rd.MonthNo
                             FROM V3M_FIN_StudentReceiptDetail rd WITH (NOLOCK)
                             WHERE rd.ReceiptId = rm.ReceiptId)
    ) per
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
      AND (@Status        = -1    OR rm.Valid = @Status)   -- Valid: 1 = active, 0 = cancelled (IsActive stays 1 after a cancel)
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

/*
EXEC dbo.V3M_FIN_PrintViewStudentReceipts
     @GroupCode = '01', @BranchCode = '01', @SessionId = 1043,
     @FromDate = '2026-10-01', @ToDate = '2026-10-31';
*/

CREATE   PROCEDURE dbo.V3M_FIN_PrintViewStudentReceipts
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @SessionId      BIGINT       = 0,
    @GroupClassCode VARCHAR(20)  = '',
    @ClassCode      VARCHAR(20)  = '',
    @PaymentMode    VARCHAR(20)  = '',
    @ReceiptMode    INT          = -1,      -- IsAdmissionReceipt: -1 = sab
    @ReceiptNo      VARCHAR(50)  = '',
    @StudentNo      VARCHAR(50)  = '',      -- ControlNo
    @FromDate       DATE         = NULL,
    @ToDate         DATE         = NULL,
    @Status         INT          = -1,      -- IsActive: -1 = sab (tab ReceiptType column aata hai)
    @FirstName      VARCHAR(100) = '',
    @SocietyId      INT          = 0,
    @ChequeNo       VARCHAR(50)  = '',
    @BankAccountId  INT          = 0,
    @FeeReceivedBy  VARCHAR(50)  = '',
    @CancelType     VARCHAR(10)  = '-1',
    @ReceiptIds     VARCHAR(MAX) = '',      -- '4' ya '4,5,9'
    @ChqType        VARCHAR(20)  = ''       -- '' = cheque filter nahi; '0' = sab cheque; else us type ke
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
    SET @ReceiptIds     = NULLIF(NULLIF(LTRIM(RTRIM(@ReceiptIds)), ''), '0');

    DECLARE @ShowChqType BIT = CASE WHEN ISNULL(@ChqType, '') <> '' THEN 1 ELSE 0 END;
    IF @ShowChqType = 1 SET @PaymentMode = 'Cheque';
    DECLARE @ChqTypeFilter VARCHAR(20) = CASE WHEN @ShowChqType = 1 AND @ChqType <> '0' THEN @ChqType END;

    ------------------------------------------------------------------
    -- 1. Data
    ------------------------------------------------------------------
    SELECT rm.ReceiptId,
           rm.ReceiptNo                                                     AS ReceiptNo,
           vs.ControlNo                                                     AS StudentNo,
           vs.StudentName                                                   AS StudentName,
           vs.ClassSection                                                  AS ClassSection,
           ISNULL(rm.TotalAmount, 0) + ISNULL(rm.PreviousBalance, 0) + ISNULL(rm.LateFee, 0) AS Total,
           rm.PaidAmount                                                    AS Paid,
           rm.Concession                                                    AS Concession,
           con.ConcessionName                                               AS ConcessionName,
           CONVERT(VARCHAR(20), rm.CreatedDate, 103)                        AS ReceiptDate,
           rm.PaymentMode                                                   AS Mode,
           CASE WHEN ISNULL(rm.ChequeType, '0') = '0' THEN '' ELSE rm.ChequeType END AS ChqType,
           rm.ChequeNo                                                      AS ChqNo,
           CASE WHEN ISNULL(rm.ChequeBankBranch, '') = '' THEN rm.ChequeBank
                ELSE rm.ChequeBank + ', ' + rm.ChequeBankBranch END         AS ChequeBank,
           CASE WHEN rm.ChequeDate IS NULL OR CAST(rm.ChequeDate AS DATE) = '1900-01-01' THEN ''
                ELSE CONVERT(VARCHAR(20), rm.ChequeDate, 103) END           AS ChequeDate,
           per.PeriodName                                                   AS PeriodName,
           rm.Remark                                                        AS Remarks,
           CASE WHEN rm.IsActive = 1 THEN 'Active' ELSE 'Cancel' END        AS ReceiptType
    INTO #T
    FROM V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
    INNER JOIN vwStudentDetails vs WITH (NOLOCK)
            ON vs.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
           AND vs.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
           AND vs.SessionId  = rm.SessionId
           AND vs.StudentId  = rm.StudentId
    OUTER APPLY
    (
        SELECT TOP 1 mc.Concession AS ConcessionName
        FROM V3M_FIN_DetConcessionMapToStudent dt WITH (NOLOCK)
        INNER JOIN V3M_FIN_MstConcession mc WITH (NOLOCK) ON mc.ConcessionId = dt.ConcessionId
        WHERE dt.GroupCode  COLLATE DATABASE_DEFAULT = vs.GroupCode  COLLATE DATABASE_DEFAULT
          AND dt.BranchCode COLLATE DATABASE_DEFAULT = vs.BranchCode COLLATE DATABASE_DEFAULT
          AND dt.SessionId = vs.SessionId
          AND dt.StudentId = vs.StudentId
          AND dt.Status = 1
    ) con
    OUTER APPLY
    (
        SELECT STRING_AGG(LEFT(mo.MonthName, 3), ',') WITHIN GROUP (ORDER BY mo.DisplayOrder) AS PeriodName
        FROM V3M_FIN_Mst_MonthDisplayOrder mo WITH (NOLOCK)
        WHERE mo.GroupCode  COLLATE DATABASE_DEFAULT = rm.GroupCode  COLLATE DATABASE_DEFAULT
          AND mo.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
          AND mo.IsValid = 1
          AND mo.MonthNo IN (SELECT DISTINCT rd.MonthNo
                             FROM V3M_FIN_StudentReceiptDetail rd WITH (NOLOCK)
                             WHERE rd.ReceiptId = rm.ReceiptId)
    ) per
    WHERE rm.GroupCode  = @GroupCode
      AND rm.BranchCode = @BranchCode
      AND (@FromDate IS NULL OR CAST(rm.CreatedDate AS DATE) BETWEEN @FromDate AND ISNULL(@ToDate, @FromDate))
      AND (@SessionId     = 0     OR rm.SessionId = @SessionId)
      AND (@BankAccountId = 0     OR rm.BankAccountId = @BankAccountId)
      AND (@FeeReceivedBy IS NULL OR rm.CreatedBy = @FeeReceivedBy)
      AND (@SocietyId     = 0     OR rm.SocietyId = @SocietyId)
      AND (@ClassCode     IS NULL OR rm.ClassCode = @ClassCode)
      AND (@PaymentMode   IS NULL OR rm.PaymentMode = @PaymentMode)
      AND (@ChqTypeFilter IS NULL OR rm.ChequeType = @ChqTypeFilter)
      AND (@StudentNo     IS NULL OR vs.ControlNo = @StudentNo)
      AND (@Status        = -1    OR rm.Valid = @Status)   -- Valid: 1 = active, 0 = cancelled (IsActive stays 1 after a cancel)
      AND (@FirstName     IS NULL OR vs.StudentName LIKE '%' + @FirstName + '%')
      AND (@ChequeNo      IS NULL OR rm.ChequeNo = @ChequeNo)
      AND (@ReceiptMode   = -1    OR rm.IsAdmissionReceipt = @ReceiptMode)
      AND (@ReceiptNo     IS NULL OR rm.ReceiptNo = @ReceiptNo)
      AND (@CancelType    IS NULL OR rm.CancelType = @CancelType)
      AND (@ReceiptIds    IS NULL OR rm.ReceiptId IN (SELECT TRY_CAST(value AS BIGINT) FROM STRING_SPLIT(@ReceiptIds, ',')))
      AND (@GroupClassCode IS NULL
           OR EXISTS (SELECT 1 FROM V3M_DetClassGroup cg WITH (NOLOCK)
                      WHERE cg.IsValid = 1 AND cg.GroupCode = @GroupCode AND cg.BranchCode = @BranchCode
                        AND cg.CGID = @GroupClassCode
                        AND cg.ClassCode COLLATE DATABASE_DEFAULT = vs.ClassCode COLLATE DATABASE_DEFAULT))
    OPTION (RECOMPILE);

    ------------------------------------------------------------------
    -- 2. Output (ChqType aur ReceiptType sirf zaroorat par)
    ------------------------------------------------------------------
    DECLARE @Sql NVARCHAR(MAX) = N'
    SELECT ROW_NUMBER() OVER (ORDER BY ReceiptId DESC) AS SNo,
           ReceiptId, ReceiptNo, StudentNo, StudentName, ClassSection,
           Total, Paid, Concession, ConcessionName, ReceiptDate, Mode'
        + CASE WHEN @ShowChqType = 1 THEN N', ChqType' ELSE N'' END
        + N', ChqNo, ChequeBank, ChequeDate, PeriodName, Remarks'
        + CASE WHEN @Status = -1 THEN N', ReceiptType' ELSE N'' END
        + N'
    FROM #T
    ORDER BY ReceiptId DESC;';

    EXEC sp_executesql @Sql;

    DROP TABLE #T;
END
GO


