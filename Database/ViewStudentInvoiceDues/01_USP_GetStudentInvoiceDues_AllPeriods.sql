/*
    USP_GetStudentInvoiceDues - "All" period support (View Invoice Dues page, /ViewStudentInvoiceDues)
    -----------------------------------------------------------------------------------------------
    Caller : Infrastructure/FinanceMNGT/FeeMNGT/StudentInvoiceService.cs -> GetStudentInvoiceDuesData
             (only caller; API StudentFeeInvoice/GetStudentInvoiceDues, page StudentInvoiceDues.razor)

    Change : the UI now sends Period Type = '' (All) and Period = 0 (All) by default.
             - @PeriodId 0 / -1 / NULL  -> every due of the student (all invoices incl. miscellaneous):
                                           balance = dbo.Fn_GetStudentBalance (open invoices + student balance)
             - @PeriodId > 0            -> only that period (unchanged: dbo.Fn_GetStudentBalanceForPeriod)
             - @PeriodType '' / NULL    -> any period type
             - @PeriodType given        -> only students of that period type (also when Period = All)
             The balance is now calculated once per student (CROSS APPLY) instead of up to 3 times.
             Everything else (columns, other filters, order) is unchanged.

    Rollback: run 01_USP_GetStudentInvoiceDues_AllPeriods_Rollback.sql (original procedure).

    Test   :
      EXEC USP_GetStudentInvoiceDues '01','01',1043,'','','',0,0,-1,'','0','','',0,0,0;           -- all dues
      EXEC USP_GetStudentInvoiceDues '01','01',1043,'','','',0,0,-1,'Quarterly','0','','',0,0,0;  -- quarterly students, all periods
      EXEC USP_GetStudentInvoiceDues '01','01',1043,'','','',1,0,-1,'Quarterly','0','','',0,0,0;  -- one period (as before)
*/
CREATE OR ALTER PROCEDURE [dbo].[USP_GetStudentInvoiceDues]
(
    @GroupCode       VARCHAR(20),
    @BranchCode      VARCHAR(20),
    @SessionId       BIGINT,
    @ClassCode       VARCHAR(MAX) = '',
    @SectionId       VARCHAR(MAX) = '',
    @StudentNo       VARCHAR(50) = '',
    @PeriodId        INT = 0,
    @SocietyId       INT = 0,
    @Status          INT = -1,
    @PeriodType      VARCHAR(20) = '',
    @DuesAmountCheck VARCHAR(50) = '0',
    @RoleName        VARCHAR(50) = '',
    @EmployeeId      VARCHAR(50) = '',
    @IsWithLateFee   BIT = 0,
    @StudentCategory INT = 0,
    @DuesAmount      DECIMAL(18,2) = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ScalerValue VARCHAR(10);
    SELECT @ScalerValue = CASE ISNULL(KeyValue,'-1') WHEN '-1' THEN '-1' WHEN '0' THEN '0' ELSE '1' END
    FROM DefaultSettings
    WHERE GroupCode = @GroupCode
      AND BranchCode = @BranchCode
      AND KeyName = 'PeriodNameOutputType';

    -- 0 / -1 / NULL = all periods; '' / NULL = all period types
    DECLARE @AllPeriods BIT = CASE WHEN ISNULL(@PeriodId, 0) IN (0, -1) THEN 1 ELSE 0 END;
    DECLARE @PeriodTypeFilter VARCHAR(20) = NULLIF(LTRIM(RTRIM(ISNULL(@PeriodType, ''))), '');

    ;WITH T1 AS
    (
        SELECT vsd.GroupCode, vsd.BloodGroup, vsd.SessionId, vsd.StudentId, vsd.RollNo, vsd.StudentNo, vsd.ControlNo, vsd.StudentName,
               vsd.ClassSection, vsd.ClassOrder, vsd.SMSMobileNo, vsd.FatherEMail, vsd.FatherName, pd.InvoiceId, pd.InvoiceNo, pd.Amount, pd.PeriodName,
               CASE WHEN ISNULL(vsd.ContactEmail,'') = ''
                    THEN CASE WHEN ISNULL(vsd.FatherEMail,'') = '' THEN ISNULL(vsd.MotherEMail,'') ELSE vsd.FatherEMail END
                    ELSE ISNULL(vsd.ContactEmail,'')
               END AS ContactEmail,
               vsd.CurrentAddress, vsd.IsValid, vsd.EWS, vsd.MotherName, vsd.IsSMSRequired, vsd.CategoryName AS Category,
               CASE WHEN @IsWithLateFee = 1
                    THEN dbo.V3M_FIN_CalculateLateFee(vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, CONVERT(VARCHAR(10), GETDATE(), 120), '')
                    ELSE 0
               END AS StudentLateFee,
               vsd.SectionName,
               ISNULL((SELECT TOP 1 CommentText FROM V3M_CommonComment CC
                       WHERE CC.GroupCode = vsd.GroupCode AND CC.BranchCode = vsd.BranchCode AND CC.SessionId = vsd.SessionId
                         AND CC.StudentNo = vsd.StudentNo AND CC.CommentType = 'Finance' AND CC.IsValid = 1), '') AS CommentText
        FROM vwStudentDetails vsd
        -- dues of the student: all invoices (All) or only the selected period
        CROSS APPLY (SELECT CASE WHEN @AllPeriods = 1
                                 THEN dbo.Fn_GetStudentBalance(vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId)
                                 ELSE dbo.Fn_GetStudentBalanceForPeriod(vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, @PeriodId)
                            END AS DuesBalance) bal
        CROSS APPLY dbo.Fn_GetStudentDuesWithPeriodName_New(vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId) AS pd
        WHERE vsd.GroupCode = @GroupCode AND vsd.BranchCode = @BranchCode
          AND vsd.SessionId = @SessionId AND (@Status = -1 OR vsd.IsValid = @Status)
          AND (@SocietyId = 0 OR vsd.SocietyId = @SocietyId)
          AND (@ClassCode = '' OR @ClassCode IS NULL OR vsd.ClassCode IN
                (SELECT Data COLLATE DATABASE_DEFAULT FROM dbo.Fn_SplitString(@ClassCode, ',')))
          AND (@SectionId = '' OR @SectionId IS NULL OR CAST(vsd.SectionId AS VARCHAR(20)) IN
                (SELECT Data COLLATE DATABASE_DEFAULT FROM dbo.Fn_SplitString(@SectionId, ',')))
          AND (@StudentNo = '' OR vsd.ControlNo = @StudentNo OR @StudentNo IS NULL)
          AND (@StudentCategory = 0 OR vsd.StudentCategory = @StudentCategory)
          AND (@PeriodTypeFilter IS NULL OR vsd.PeriodType = @PeriodTypeFilter)
          AND bal.DuesBalance > 0
          AND (@DuesAmountCheck = '0'
               OR (@DuesAmountCheck = '1' AND bal.DuesBalance > @DuesAmount)
               OR (@DuesAmountCheck = '2' AND bal.DuesBalance < @DuesAmount))
    )
    SELECT T1.* FROM T1 ORDER BY ClassOrder, SectionName, RollNo, StudentName;
END
GO
