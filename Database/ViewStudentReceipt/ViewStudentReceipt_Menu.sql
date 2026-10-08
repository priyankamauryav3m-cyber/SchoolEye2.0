/*
    ViewStudentReceipt — menu entry
    ----------------------------------------------------------------
    Adds the "View Student Receipt" activity (/ViewStudentReceipt) under
    Fee Management -> "View Receipts" feature, so the page opens from the side menu.
    After running, give it to roles from Role Master -> Menu Activity.

    Safety  : Additive, guarded (runs once). No existing row is changed.
    Rollback: see bottom (commented out).
*/

SET NOCOUNT ON;
GO

DECLARE @FeatureId INT =
(
    SELECT TOP 1 f.FeatureId
    FROM MstFeaturesList f
    INNER JOIN MstModule m ON m.ModuleId = f.ModuleId
    WHERE f.FeaturesName = 'View Receipts' AND m.MName = 'Fee Management'
);

IF @FeatureId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM MstActivityList WHERE URL = '/ViewStudentReceipt')
BEGIN
    INSERT INTO MstActivityList (ActivityName, DisplayName, DisplayOrder, IsValid, FeatureId, CreatedBy, CreatedDate, URL)
    VALUES ('View Student Receipt', 'View Student Receipt', 1, 1, @FeatureId, 'Migration', GETDATE(), '/ViewStudentReceipt');
END
GO

/* ROLLBACK (run manually only if needed)
DELETE AM FROM MST_ACMapping AM
INNER JOIN MstActivityList MA ON MA.ActivityId = AM.ActivityId
WHERE MA.URL = '/ViewStudentReceipt';
DELETE FROM MstActivityList WHERE URL = '/ViewStudentReceipt';
GO
*/
