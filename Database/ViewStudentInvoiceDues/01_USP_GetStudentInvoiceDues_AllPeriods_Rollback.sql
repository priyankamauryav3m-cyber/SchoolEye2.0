/*
    Rollback for 01_USP_GetStudentInvoiceDues_AllPeriods.sql
    Restores the original USP_GetStudentInvoiceDues (as taken from the database on 2026-11-03).
    Note: with this version the View Invoice Dues page must send a real Period (> 0) again.
*/
--Exec USP_GetStudentInvoiceDues '01','01',1043,'','','','1',0,-1,'Quarterly','0','SMSNo','FatherNo',1,0,0
CREATE OR ALTER PROCEDURE [dbo].[USP_GetStudentInvoiceDues]
(
    @GroupCode VARCHAR(20), @BranchCode VARCHAR(20), @SessionId bigint, @ClassCode VARCHAR(MAX) = '',
    @SectionId VARCHAR(MAX) = '', @StudentNo VARCHAR(50) = '', @PeriodId INT, @SocietyId int = 0, @Status INT = -1, @PeriodType VARCHAR(20) = '',
    @DuesAmountCheck VARCHAR(50) ='0', @RoleName VARCHAR(50) = '',
    @EmployeeId VARCHAR(50) = '', @IsWithLateFee BIT = 0, @StudentCategory Int= 0,@DuesAmount decimal(18,2)=0
)
AS
BEGIN
    DECLARE @ScalerValue VARCHAR(10);
    SELECT @ScalerValue = CASE ISNULL(KeyValue,'-1') WHEN '-1' THEN '-1' WHEN '0' THEN '0' ELSE '1' END
    FROM DefaultSettings
    WHERE GroupCode  =@GroupCode
     AND BranchCode  =@BranchCode
     AND KeyName  ='PeriodNameOutputType';
    ;WITH T1 AS
    (
        SELECT vsd.GroupCode,vsd.BloodGroup,vsd.SessionId,vsd.StudentId,vsd.RollNo,vsd.StudentNo,vsd.ControlNo,vsd.StudentName,
           vsd.ClassSection,vsd.ClassOrder,vsd.SMSMobileNo,vsd.FatherEMail,vsd.FatherName,pd.InvoiceId,pd.InvoiceNo,pd.Amount,pd.PeriodName,
            CASE
         WHEN ISNULL(vsd.ContactEmail,'') = ''
                 THEN CASE WHEN ISNULL(vsd.FatherEMail,'') = '' THEN ISNULL(vsd.MotherEMail,'') ELSE vsd.FatherEMail END
            ELSE ISNULL(vsd.ContactEmail,'')
           END      AS ContactEmail, vsd.CurrentAddress, vsd.IsValid, vsd.EWS,vsd.MotherName, vsd.IsSMSRequired,vsd.CategoryName as Category,
            --dbo.Fn_GetStudentDuesWithPeriodName_New(vsd.GroupCode,vsd.BranchCode,vsd.SessionId,vsd.StudentId) AS PeriodName,
           CASE
      WHEN @IsWithLateFee = 1 THEN dbo.V3M_FIN_CalculateLateFee ( vsd.GroupCode, vsd.BranchCode, vsd.SessionId,vsd.StudentId, CONVERT(VARCHAR(10),GETDATE(),120), '' )
      ELSE 0
     END AS StudentLateFee,vsd.SectionName,
            ISNULL( ( SELECT TOP 1 CommentText FROM V3M_CommonComment CC WHERE CC.GroupCode  =
        vsd.GroupCode  AND CC.BranchCode  = vsd.BranchCode  AND  CC.SessionId  =vsd.SessionId
         AND CC.StudentNo  =vsd.StudentNo  AND CC.CommentType = 'Finance' AND CC.IsValid = 1),'') AS CommentText FROM vwStudentDetails vsd
  CROSS APPLY dbo.Fn_GetStudentDuesWithPeriodName_New ( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId ) As pd
        WHERE vsd.GroupCode  = @GroupCode AND vsd.BranchCode  =@BranchCode
           AND vsd.SessionId  =@SessionId AND (@Status = -1 OR vsd.IsValid = @Status)
          AND ( @SocietyId = 0 OR vsd.SocietyId  = @SocietyId )
          AND (@ClassCode = ''OR @ClassCode Is Null OR vsd.ClassCode  IN
                (SELECT Data COLLATE DATABASE_DEFAULT FROM dbo.Fn_SplitString(@ClassCode, ',')) )
          AND ( @SectionId = '' OR @SectionId Is Null OR   CAST(vsd.SectionId AS VARCHAR(20)) IN
          (SELECT Data COLLATE DATABASE_DEFAULT FROM dbo.Fn_SplitString(@SectionId, ',') ))
          AND ( @StudentNo = ''OR vsd.ControlNo =@StudentNo OR @StudentNo Is Null )
          AND ( @StudentCategory =0 OR vsd.StudentCategory  =@StudentCategory )
         AND ( @PeriodId IN ('0','-1') OR vsd.PeriodType = @PeriodType )
         AND dbo.Fn_GetStudentBalanceForPeriod ( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, @PeriodId ) > 0
        AND ( @DuesAmountCheck = '0' OR (@DuesAmountCheck = '1' AND dbo.Fn_GetStudentBalanceForPeriod( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, @PeriodId ) > @DuesAmount)
       OR (@DuesAmountCheck = '2' AND dbo.Fn_GetStudentBalanceForPeriod( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, @PeriodId ) < @DuesAmount) ))
       -- SELECT T1.*,CAST((SELECT Data FROM dbo.Fn_SplitString(PeriodName,'~') WHERE Id = 1) AS DECIMAL(18,2)) AS Amount,
       --(SELECT Data FROM dbo.Fn_SplitString(PeriodName,'~') WHERE Id = 2 ) AS Period
       --  FROM T1 ORDER BY ClassOrder, SectionName, RollNo, StudentName;
       SELECT T1.* FROM T1 ORDER BY ClassOrder, SectionName, RollNo, StudentName
END
GO
