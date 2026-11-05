/*
    V3M_GetReportHeader : header (school name / address / phone / email / website) for every PDF / Excel.
    Reads only V3M_MstReportHeader (IsValid = 1):
      1. exact GroupCode + BranchCode (ReportName 'Default' first)
      2. else any record of the same GroupCode
      3. else the first valid record
    Called by Infrastructure ReportHeaderService (API).  New procedure: no existing caller changes.
    Rollback: DROP PROCEDURE dbo.V3M_GetReportHeader;
*/
CREATE OR ALTER PROCEDURE dbo.V3M_GetReportHeader
(
    @GroupCode  VARCHAR(10) = NULL,
    @BranchCode VARCHAR(10) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @GroupCode  = NULLIF(LTRIM(RTRIM(@GroupCode)), '');
    SET @BranchCode = NULLIF(LTRIM(RTRIM(@BranchCode)), '');

    SELECT TOP 1
           ISNULL(rh.GroupCode, '')              AS GroupCode,
           ISNULL(rh.BranchCode, '')             AS BranchCode,
           ISNULL(rh.HeaderRow1, '')             AS SchoolName,
           ISNULL(rh.HeaderRow2, '')             AS Address,
           ISNULL(rh.HeaderRow3, '')             AS Phone,
           ISNULL(rh.HeaderRow4, '')             AS Email,
           ISNULL(rh.HeaderRow5, '')             AS Website,
           CAST(ISNULL(rh.LogoInHeader, 1) AS BIT) AS LogoInHeader
    FROM dbo.V3M_MstReportHeader rh WITH (NOLOCK)
    WHERE rh.IsValid = 1
    ORDER BY
        CASE
            WHEN rh.GroupCode = @GroupCode AND rh.BranchCode = @BranchCode THEN 0
            WHEN rh.GroupCode = @GroupCode THEN 1
            ELSE 2
        END,
        CASE WHEN rh.ReportName = 'Default' THEN 0 ELSE 1 END,
        rh.RID;
END
