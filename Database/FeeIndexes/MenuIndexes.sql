/*
    MenuIndexes — side menu load speed (V3M_RoleMenuAndPermission)
    ----------------------------------------------------------------
    Caller  : SuperAdminService.GetRoleBasedActivity -> GET SuperAdminModule/RoleBasedActivity/{roleId}
              -> NavMenu.razor (menu + PermissionState). Runs on every login / page refresh.

    Why     : The SP filters MST_ACMapping by RoleId and joins MSTModule / MstFeaturesList /
              MstActivityList by their Ids. Without an index on MST_ACMapping.RoleId the whole
              mapping table (all roles) is scanned on every menu load.

    Changes : Only additive NONCLUSTERED indexes. Each one is created only when the table has
              no index whose FIRST key column is already that column (no duplicate indexes).
              No table, column, data or procedure is changed. Safe to run multiple times.

    Rollback: MenuIndexes_Rollback.sql (drops only the indexes created here, by name).
*/

SET NOCOUNT ON;
GO

-- 1. MST_ACMapping: WHERE AM.RoleId = @RoleId (covers every column the SP reads)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID('dbo.MST_ACMapping') AND c.name = 'RoleId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MST_ACMapping_RoleId
        ON dbo.MST_ACMapping (RoleId)
        INCLUDE (ModuleId, FeatureId, ActivityId,
                 IsAdd, IsModifiy, IsPrint, IsExportToExcel, IsPII,
                 Action1, Action2, Action3);
    PRINT 'Created IX_MST_ACMapping_RoleId';
END
ELSE
    PRINT 'Skipped MST_ACMapping: an index on RoleId already exists';
GO

-- 2. Join keys of the master tables (normally already the primary key -> skipped)
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID('dbo.MstActivityList') AND c.name = 'ActivityId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MstActivityList_ActivityId ON dbo.MstActivityList (ActivityId);
    PRINT 'Created IX_MstActivityList_ActivityId';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID('dbo.MstFeaturesList') AND c.name = 'FeatureId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MstFeaturesList_FeatureId ON dbo.MstFeaturesList (FeatureId);
    PRINT 'Created IX_MstFeaturesList_FeatureId';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
    JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID('dbo.MSTModule') AND c.name = 'ModuleId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MSTModule_ModuleId ON dbo.MSTModule (ModuleId);
    PRINT 'Created IX_MSTModule_ModuleId';
END
GO

-- Check: EXEC dbo.V3M_RoleMenuAndPermission @RoleId = 8  (with "Include Actual Execution Plan" -> Index Seek on MST_ACMapping)
