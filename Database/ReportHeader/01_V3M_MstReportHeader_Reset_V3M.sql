/*
    Report header (all PDF / Excel headers) -> V3M_MstReportHeader
    ------------------------------------------------------------------
    1. Backup of the current rows  -> V3M_MstReportHeader_Bkp_20261105  (only if the backup table is not there yet)
    2. TRUNCATE V3M_MstReportHeader (old Cambridge rows, GroupCode 18)
    3. One record: GroupCode 01, BranchCode 01, ReportName 'Default' = V3M International School

    Column use (API: ReportHeaderService -> V3M_GetReportHeader):
        HeaderRow1 = School name
        HeaderRow2 = Address
        HeaderRow3 = Phone
        HeaderRow4 = Email          (blank = line not shown)
        HeaderRow5 = Website        (blank = line not shown)
        LogoInHeader = 1

    Rollback: 01_V3M_MstReportHeader_Reset_V3M_Rollback.sql
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.V3M_MstReportHeader_Bkp_20261105', 'U') IS NULL
BEGIN
    SELECT *
    INTO dbo.V3M_MstReportHeader_Bkp_20261105
    FROM dbo.V3M_MstReportHeader;
END

TRUNCATE TABLE dbo.V3M_MstReportHeader;

INSERT INTO dbo.V3M_MstReportHeader
    (GroupCode, BranchCode, SocietyId, ReportName,
     HeaderRow1, HeaderRow2, HeaderRow3, HeaderRow4, HeaderRow5, HeaderRow6, HeaderRow7, HeaderRow8,
     HeaderAlign, LogoInHeader, LogoAlign, IsValid, CreatedBy, CreatedDate)
VALUES
    ('01', '01', 0, 'Default',
     'V3M International School',
     'FF-231 232, Palam Corporate Plaza, Palam Vihar, 122017',
     '08373918357',
     '',
     '',
     '', '', '',
     '1', 1, '1', 1, 'V3M', GETDATE());

COMMIT TRANSACTION;

SELECT RID, GroupCode, BranchCode, ReportName, HeaderRow1, HeaderRow2, HeaderRow3, HeaderRow4, HeaderRow5, IsValid
FROM dbo.V3M_MstReportHeader;
