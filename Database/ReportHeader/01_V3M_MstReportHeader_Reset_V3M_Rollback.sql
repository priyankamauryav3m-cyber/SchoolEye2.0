/*
    Rollback of 01_V3M_MstReportHeader_Reset_V3M.sql
    Puts back the old rows from V3M_MstReportHeader_Bkp_20261105 (same RID values).
*/
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.V3M_MstReportHeader_Bkp_20261105', 'U') IS NULL
BEGIN
    RAISERROR('Backup table V3M_MstReportHeader_Bkp_20261105 not found - nothing to restore.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

TRUNCATE TABLE dbo.V3M_MstReportHeader;

SET IDENTITY_INSERT dbo.V3M_MstReportHeader ON;

INSERT INTO dbo.V3M_MstReportHeader
    (RID, GroupCode, BranchCode, SocietyId, ReportName,
     HeaderRow1, HeaderRow2, HeaderRow3, HeaderRow4, HeaderRow5, HeaderRow6, HeaderRow7, HeaderRow8,
     HeaderAlign, LogoInHeader, LogoAlign, IsValid, CreatedBy, CreatedDate)
SELECT RID, GroupCode, BranchCode, SocietyId, ReportName,
       HeaderRow1, HeaderRow2, HeaderRow3, HeaderRow4, HeaderRow5, HeaderRow6, HeaderRow7, HeaderRow8,
       HeaderAlign, LogoInHeader, LogoAlign, IsValid, CreatedBy, CreatedDate
FROM dbo.V3M_MstReportHeader_Bkp_20261105;

SET IDENTITY_INSERT dbo.V3M_MstReportHeader OFF;

COMMIT TRANSACTION;
