/*
    ViewStudentDuesAndReceipt — menu entry
    ----------------------------------------------------------------
    The old "View Student Receipt" page was renamed to /ViewStudentDuesAndReceipt.
    The existing activity /ViewStudentReceipt now opens the NEW View Student Receipt page.
    This adds "View Student Dues And Receipt" (/ViewStudentDuesAndReceipt) under
    Fee Management -> "View Receipts" feature, so the old page still opens from the side menu.
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
   AND NOT EXISTS (SELECT 1 FROM MstActivityList WHERE URL = '/ViewStudentDuesAndReceipt')
BEGIN
    INSERT INTO MstActivityList (ActivityName, DisplayName, DisplayOrder, IsValid, FeatureId, CreatedBy, CreatedDate, URL)
    VALUES ('View Student Dues And Receipt', 'View Student Dues And Receipt', 2, 1, @FeatureId, 'Migration', GETDATE(), '/ViewStudentDuesAndReceipt');
END
GO

/* ROLLBACK (run manually only if needed)
DELETE AM FROM MST_ACMapping AM
INNER JOIN MstActivityList MA ON MA.ActivityId = AM.ActivityId
WHERE MA.URL = '/ViewStudentDuesAndReceipt';
DELETE FROM MstActivityList WHERE URL = '/ViewStudentDuesAndReceipt';
GO
*/
