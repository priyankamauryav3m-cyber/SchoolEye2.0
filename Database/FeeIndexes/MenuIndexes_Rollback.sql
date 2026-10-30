/*
    Rollback for MenuIndexes.sql — drops only the indexes that script created (by name).
    Existing indexes / primary keys are never touched.
*/

DROP INDEX IF EXISTS IX_MST_ACMapping_RoleId       ON dbo.MST_ACMapping;
DROP INDEX IF EXISTS IX_MstActivityList_ActivityId ON dbo.MstActivityList;
DROP INDEX IF EXISTS IX_MstFeaturesList_FeatureId  ON dbo.MstFeaturesList;
DROP INDEX IF EXISTS IX_MSTModule_ModuleId         ON dbo.MSTModule;
GO
