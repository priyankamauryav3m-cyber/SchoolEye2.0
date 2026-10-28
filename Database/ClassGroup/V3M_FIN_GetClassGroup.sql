/*
    V3M_FIN_GetClassGroup
    ----------------------------------------------------------------
    Class group list for the View Student Receipt page (Class-Group dropdown).
    Source  : dbo.V3M_DetClassGroup, filtered by GroupCode + BranchCode.
    Caller  : StudentFeeReceipt/GetClassGroup

    Safety  : New read-only procedure. No table / data is changed.
    Rollback: see bottom (commented out).
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetClassGroup
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        cg.ClassCode
    FROM dbo.V3M_DetClassGroup cg
    WHERE cg.GroupCode  = @GroupCode
      AND cg.BranchCode = @BranchCode
      AND ISNULL(cg.ClassCode, '') <> ''
    ORDER BY cg.ClassCode;
END
GO

/* ROLLBACK (run manually only if needed)
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_GetClassGroup;
GO
*/
