/*
    RoleMenuOrder — role-wise menu ordering (Module -> Feature -> Activity)
    ----------------------------------------------------------------
    Purpose : Lets an admin set, per role, the order in which Modules, Features and
              Activities appear in the side menu (page: /menumappingwithrolebase).

    Design  : New table Mst_RoleMenuOrder stores only overrides. If a role has no
              override for an item, the existing global DisplayOrder
              (MstModule / MstFeaturesList / MstActivityList) is used as before.

    Changes : 1. CREATE TABLE  dbo.Mst_RoleMenuOrder                 (additive)
              2. CREATE PROC   dbo.V3M_RoleMenuOrder_GetByRole        (new)
              3. CREATE PROC   dbo.V3M_RoleMenuOrder_Save             (new)
              4. CREATE PROC   dbo.V3M_RoleMenuOrder_Reset            (new)
              5. ALTER  PROC   dbo.V3M_RoleMenuAndPermission          (ORDER BY now honours role override)
                 Caller checked: SuperAdminService.GetRoleBasedActivity -> NavMenu.razor (menu + PermissionState).
                 Output columns are unchanged (same names/types); only ModuleOrder, FeatureOrder and
                 DisplayOrder values may now come from the role override.
              6. INSERT menu activity "Menu Order" (/menumappingwithrolebase) under the feature that
                 already holds /rolemaster — only if not present.

    Safety  : No table/column is dropped or altered. Safe to run multiple times (guarded).

    Rollback: See bottom of this file (commented out). Restores the original
              V3M_RoleMenuAndPermission and drops only the objects created here.
*/

SET NOCOUNT ON;
GO

-- ============================================================
-- 1. Table: Mst_RoleMenuOrder
--    LevelType: 'M' = Module, 'F' = Feature, 'A' = Activity
--    RefId    : ModuleId / FeatureId / ActivityId (per LevelType)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Mst_RoleMenuOrder')
BEGIN
    CREATE TABLE dbo.Mst_RoleMenuOrder
    (
        RoleMenuOrderId INT IDENTITY(1,1) NOT NULL,
        RoleId          INT           NOT NULL,
        LevelType       CHAR(1)       NOT NULL,
        RefId           INT           NOT NULL,
        DisplayOrder    INT           NOT NULL,
        CreatedBy       NVARCHAR(200) NULL,
        CreatedDate     DATETIME      NOT NULL CONSTRAINT DF_Mst_RoleMenuOrder_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy       NVARCHAR(200) NULL,
        UpdatedDate     DATETIME      NULL,

        CONSTRAINT PK_Mst_RoleMenuOrder PRIMARY KEY CLUSTERED (RoleMenuOrderId),
        CONSTRAINT CK_Mst_RoleMenuOrder_LevelType CHECK (LevelType IN ('M', 'F', 'A')),
        CONSTRAINT CK_Mst_RoleMenuOrder_DisplayOrder CHECK (DisplayOrder BETWEEN 0 AND 9999)
    );

    -- One order per (role, level, item). Also serves the LEFT JOIN lookups in the menu SP.
    CREATE UNIQUE NONCLUSTERED INDEX UX_Mst_RoleMenuOrder_Role_Level_Ref
        ON dbo.Mst_RoleMenuOrder (RoleId, LevelType, RefId)
        INCLUDE (DisplayOrder);
END
GO

-- ============================================================
-- 2. V3M_RoleMenuOrder_GetByRole
--    Returns the role's mapped menu tree with the effective order
--    (role override if set, otherwise the global DisplayOrder).
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.V3M_RoleMenuOrder_GetByRole
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        MM.ModuleId,
        MM.MName,
        ISNULL(OM.DisplayOrder, ISNULL(MM.DisplayOrder, 0)) AS ModuleOrder,
        CAST(CASE WHEN OM.RoleMenuOrderId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsModuleCustom,
        MF.FeatureId,
        MF.FeaturesName,
        ISNULL(OFt.DisplayOrder, ISNULL(MF.DisplayOrder, 0)) AS FeatureOrder,
        CAST(CASE WHEN OFt.RoleMenuOrderId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsFeatureCustom,
        MA.ActivityId,
        MA.ActivityName,
        MA.DisplayName,
        ISNULL(OA.DisplayOrder, ISNULL(MA.DisplayOrder, 0)) AS ActivityOrder,
        CAST(CASE WHEN OA.RoleMenuOrderId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsActivityCustom
    FROM MST_ACMapping   AM WITH (NOLOCK)
    INNER JOIN MSTModule       MM WITH (NOLOCK) ON MM.ModuleId   = AM.ModuleId
    INNER JOIN MstFeaturesList MF WITH (NOLOCK) ON MF.FeatureId  = AM.FeatureId
    INNER JOIN MstActivityList MA WITH (NOLOCK) ON MA.ActivityId = AM.ActivityId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OM  WITH (NOLOCK) ON OM.RoleId  = AM.RoleId AND OM.LevelType  = 'M' AND OM.RefId  = MM.ModuleId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OFt WITH (NOLOCK) ON OFt.RoleId = AM.RoleId AND OFt.LevelType = 'F' AND OFt.RefId = MF.FeatureId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OA  WITH (NOLOCK) ON OA.RoleId  = AM.RoleId AND OA.LevelType  = 'A' AND OA.RefId  = MA.ActivityId
    WHERE AM.RoleId = @RoleId
    ORDER BY ModuleOrder, MM.ModuleId, FeatureOrder, MF.FeatureId, ActivityOrder, MA.ActivityId;
END
GO

-- ============================================================
-- 3. V3M_RoleMenuOrder_Save
--    @JsonData: [{"LevelType":"M","RefId":2,"DisplayOrder":1}, ...]
--    Upserts the given rows for the role in one transaction.
--    Returns SavedCount (0 = nothing saved).
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.V3M_RoleMenuOrder_Save
    @RoleId    INT,
    @CreatedBy NVARCHAR(200),
    @JsonData  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM MstRoles WITH (NOLOCK) WHERE RoleId = @RoleId)
    BEGIN
        SELECT -1 AS SavedCount;   -- role not found
        RETURN;
    END

    DECLARE @Src TABLE
    (
        LevelType    CHAR(1) NOT NULL,
        RefId        INT     NOT NULL,
        DisplayOrder INT     NOT NULL,
        PRIMARY KEY (LevelType, RefId)
    );

    INSERT INTO @Src (LevelType, RefId, DisplayOrder)
    SELECT DISTINCT j.LevelType, j.RefId, j.DisplayOrder
    FROM OPENJSON(@JsonData)
    WITH
    (
        LevelType    CHAR(1) '$.LevelType',
        RefId        INT     '$.RefId',
        DisplayOrder INT     '$.DisplayOrder'
    ) j
    WHERE j.LevelType IN ('M', 'F', 'A')
      AND j.RefId > 0
      AND j.DisplayOrder BETWEEN 0 AND 9999;

    DECLARE @Saved INT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE tgt
        SET tgt.DisplayOrder = src.DisplayOrder,
            tgt.UpdatedBy    = @CreatedBy,
            tgt.UpdatedDate  = GETDATE()
        FROM dbo.Mst_RoleMenuOrder tgt
        INNER JOIN @Src src ON src.LevelType = tgt.LevelType AND src.RefId = tgt.RefId
        WHERE tgt.RoleId = @RoleId;
        SET @Saved = @@ROWCOUNT;

        INSERT INTO dbo.Mst_RoleMenuOrder (RoleId, LevelType, RefId, DisplayOrder, CreatedBy)
        SELECT @RoleId, src.LevelType, src.RefId, src.DisplayOrder, @CreatedBy
        FROM @Src src
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Mst_RoleMenuOrder tgt
            WHERE tgt.RoleId = @RoleId AND tgt.LevelType = src.LevelType AND tgt.RefId = src.RefId
        );
        SET @Saved = @Saved + @@ROWCOUNT;

        COMMIT;
        SELECT @Saved AS SavedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- 4. V3M_RoleMenuOrder_Reset
--    Removes the role's overrides -> menu falls back to global DisplayOrder.
--    Touches only Mst_RoleMenuOrder (never access mapping).
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.V3M_RoleMenuOrder_Reset
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Mst_RoleMenuOrder WHERE RoleId = @RoleId;
    SELECT @@ROWCOUNT AS ResetCount;
END
GO

-- ============================================================
-- 5. V3M_RoleMenuAndPermission — same output columns, role-wise order
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[V3M_RoleMenuAndPermission]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MM.ModuleId,
        MM.MName,
        ISNULL(OM.DisplayOrder, MM.DisplayOrder)   AS ModuleOrder,
        MF.FeatureId,
        MF.FeaturesName,
        ISNULL(MF.Icon, 'bi bi-circle')            AS Icon,
        ISNULL(OFt.DisplayOrder, MF.DisplayOrder)  AS FeatureOrder,
        MA.ActivityId,
        MA.ActivityName,
        CAST(ISNULL(MM.IsRestricted, 0) AS INT)    AS IsRestricted,
        MA.URL,
        ISNULL(OA.DisplayOrder, MA.DisplayOrder)   AS DisplayOrder,
        MA.DisplayName,
        MA.LabelIcon,
        AM.IsAdd, AM.IsModifiy, AM.IsPrint, AM.IsExportToExcel, AM.IsPII,
        AM.Action1, AM.Action2, AM.Action3
    FROM MstRoles RS WITH (NOLOCK)
    INNER JOIN MST_ACMapping   AM WITH (NOLOCK) ON AM.RoleId     = RS.RoleId
    INNER JOIN MSTModule       MM WITH (NOLOCK) ON MM.ModuleId   = AM.ModuleId
    INNER JOIN MstFeaturesList MF WITH (NOLOCK) ON MF.FeatureId  = AM.FeatureId
    INNER JOIN MstActivityList MA WITH (NOLOCK) ON MA.ActivityId = AM.ActivityId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OM  WITH (NOLOCK) ON OM.RoleId  = RS.RoleId AND OM.LevelType  = 'M' AND OM.RefId  = MM.ModuleId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OFt WITH (NOLOCK) ON OFt.RoleId = RS.RoleId AND OFt.LevelType = 'F' AND OFt.RefId = MF.FeatureId
    LEFT  JOIN dbo.Mst_RoleMenuOrder OA  WITH (NOLOCK) ON OA.RoleId  = RS.RoleId AND OA.LevelType  = 'A' AND OA.RefId  = MA.ActivityId
    WHERE RS.RoleId = @RoleId
    ORDER BY ISNULL(OM.DisplayOrder, MM.DisplayOrder),  MM.ModuleId,
             ISNULL(OFt.DisplayOrder, MF.DisplayOrder), MF.FeatureId,
             ISNULL(OA.DisplayOrder, MA.DisplayOrder),  MA.ActivityId;
END
-- EXEC V3M_RoleMenuAndPermission 8
GO

-- ============================================================
-- 6. Menu entry for the new page (same feature as /rolemaster).
--    Assign it to roles from Role Master -> Menu Activity.
-- ============================================================
DECLARE @RoleFeatureId INT = (SELECT TOP 1 FeatureId FROM MstActivityList WHERE URL = '/rolemaster');

IF @RoleFeatureId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM MstActivityList WHERE URL = '/menumappingwithrolebase')
BEGIN
    INSERT INTO MstActivityList (ActivityName, DisplayName, DisplayOrder, IsValid, FeatureId, CreatedBy, CreatedDate, URL)
    VALUES ('Menu Order', 'Menu Order', 3, 1, @RoleFeatureId, 'Migration', GETDATE(), '/menumappingwithrolebase');
END
GO


/* ============================================================
   ROLLBACK (run manually only if needed)
   ============================================================

-- a) Restore original V3M_RoleMenuAndPermission
CREATE OR ALTER PROCEDURE [dbo].[V3M_RoleMenuAndPermission]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MM.ModuleId,
        MM.MName,
        MM.DisplayOrder                     AS ModuleOrder,
        MF.FeatureId,
        MF.FeaturesName,
        ISNULL(MF.Icon, 'bi bi-circle')     AS Icon,
        MF.DisplayOrder                     AS FeatureOrder,
        MA.ActivityId,
        MA.ActivityName,
        CAST(ISNULL(MM.IsRestricted, 0) AS INT) AS IsRestricted,
        MA.URL,
        MA.DisplayOrder,
        MA.DisplayName,
        MA.LabelIcon,
        AM.IsAdd, AM.IsModifiy, AM.IsPrint, AM.IsExportToExcel, AM.IsPII,
        AM.Action1, AM.Action2, AM.Action3
    FROM MstRoles RS WITH (NOLOCK)
    INNER JOIN MST_ACMapping   AM WITH (NOLOCK) ON AM.RoleId     = RS.RoleId
    INNER JOIN MSTModule       MM WITH (NOLOCK) ON MM.ModuleId   = AM.ModuleId
    INNER JOIN MstFeaturesList MF WITH (NOLOCK) ON MF.FeatureId  = AM.FeatureId
    INNER JOIN MstActivityList MA WITH (NOLOCK) ON MA.ActivityId = AM.ActivityId
    WHERE RS.RoleId = @RoleId
    ORDER BY MM.DisplayOrder, MM.ModuleId,
             MF.DisplayOrder, MF.FeatureId,
             MA.DisplayOrder, MA.ActivityId;
END
GO

-- b) Remove the menu entry (and its role mappings) for the new page
DELETE AM FROM MST_ACMapping AM
INNER JOIN MstActivityList MA ON MA.ActivityId = AM.ActivityId
WHERE MA.URL = '/menumappingwithrolebase';
DELETE FROM MstActivityList WHERE URL = '/menumappingwithrolebase';
GO

-- c) Drop new objects
DROP PROCEDURE IF EXISTS dbo.V3M_RoleMenuOrder_Reset;
DROP PROCEDURE IF EXISTS dbo.V3M_RoleMenuOrder_Save;
DROP PROCEDURE IF EXISTS dbo.V3M_RoleMenuOrder_GetByRole;
DROP TABLE     IF EXISTS dbo.Mst_RoleMenuOrder;
GO
*/
