/* ============================================================================
   Optional Fee Group - class wise fee head mapping (/OptionalFeeGroupMaster, Map Group popup)

   Before : V3M_FIN_MstOptionalFeeGroup (group)
              -> V3M_FIN_DetClassOptionalGroup (class, FeeHeadCount, TillDate)
                 -> V3M_FIN_DetOptionalGroupFeeHeadMapping (fee head, amount) via ClassOptionalGroupId
   After  : the mapping table carries the group, class and till date itself:
              V3M_FIN_DetOptionalGroupFeeHeadMapping (OptFeeGroupId, ClassCode, FeeHeadId, Amount, TillDate, IsValid)
            V3M_FIN_DetClassOptionalGroup is no longer written by the new screen (table and its data are kept).

   1) new columns OptFeeGroupId, ClassCode, TillDate (NULL-able, nothing dropped)
      + backfill of the old rows from V3M_FIN_DetClassOptionalGroup (only rows where OptFeeGroupId IS NULL)
      + ClassOptionalGroupId made NULL-able (new rows have no class-group row)
   2) TVP dbo.FIN_OptionalGroupFeeHeadType (FeeHeadId, Amount)
   3) USp_FIN_DetOptionalGroupFeeHeadMapping   (CHANGED - map / update one class with N fee heads)
   4) USp_FIN_UnmapOptionalGroupClass          (new - unmap one class)
   5) USp_FIN_GetOptionalGroupFeeHeadMapping   (new - mapped classes / heads of one group)
   6) index for the class lookup

   !! USp_FIN_DetOptionalGroupFeeHeadMapping already exists and its parameters change.
      Before running: script out the current procedure (SSMS > Script Procedure as > CREATE) and keep it
      as the rollback; check that the old application does not still call it.

   Rollback:
      restore the old USp_FIN_DetOptionalGroupFeeHeadMapping from the saved script;
      DROP PROCEDURE dbo.USp_FIN_UnmapOptionalGroupClass;
      DROP PROCEDURE dbo.USp_FIN_GetOptionalGroupFeeHeadMapping;
      DROP TYPE dbo.FIN_OptionalGroupFeeHeadType;   (after the procedures that use it)
      DROP INDEX IX_FIN_DetOptGroupFeeHeadMap_GroupClass ON dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping;
      The new columns can stay (NULL-able, old code ignores them); drop them only with approval.
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- 1) columns ---------- */
IF COL_LENGTH('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping', 'OptFeeGroupId') IS NULL
    ALTER TABLE dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping ADD OptFeeGroupId INT NULL;
GO
IF COL_LENGTH('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping', 'ClassCode') IS NULL
    ALTER TABLE dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping ADD ClassCode VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping', 'TillDate') IS NULL
    ALTER TABLE dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping ADD TillDate DATE NULL;
GO

-- new rows have no V3M_FIN_DetClassOptionalGroup row: ClassOptionalGroupId must accept NULL (type kept as it is)
IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping')
              AND name = 'ClassOptionalGroupId' AND is_nullable = 0)
BEGIN
    DECLARE @type NVARCHAR(100) =
        (SELECT TYPE_NAME(c.user_type_id) FROM sys.columns c
          WHERE c.object_id = OBJECT_ID('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping') AND c.name = 'ClassOptionalGroupId');
    DECLARE @sql NVARCHAR(400) =
        N'ALTER TABLE dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping ALTER COLUMN ClassOptionalGroupId ' + QUOTENAME(@type) + N' NULL;';
    EXEC sp_executesql @sql;
END
GO

-- old rows: group / class / till date from V3M_FIN_DetClassOptionalGroup
UPDATE m
   SET m.OptFeeGroupId = c.OptFeeGroupId,
       m.ClassCode     = c.ClassCode,
       m.TillDate      = c.TillDate
  FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping AS m
  INNER JOIN dbo.V3M_FIN_DetClassOptionalGroup AS c ON c.SeedId = m.ClassOptionalGroupId
 WHERE m.OptFeeGroupId IS NULL;
GO

/* ---------- 2) TVP ---------- */
IF TYPE_ID('dbo.FIN_OptionalGroupFeeHeadType') IS NULL
    CREATE TYPE dbo.FIN_OptionalGroupFeeHeadType AS TABLE
    (
        FeeHeadId INT           NOT NULL PRIMARY KEY,
        Amount    DECIMAL(18,2) NOT NULL
    );
GO

/* ---------- 3) map / update one class ----------
   every fee head of @FeeHeads -> one active row (OptFeeGroupId + ClassCode + FeeHeadId):
     already there -> Amount / TillDate updated (re-activated if it was unmapped)
     not there     -> inserted
   active heads of the class that are NOT in @FeeHeads -> IsValid = 0 (e.g. 3 heads -> 2)
   @Status  1 mapped (class had no active head), 2 updated,
           -1 group not found / not active in this branch, -2 no fee head, -3 invalid fee head / amount,
           -4 till date missing, -99 error */
CREATE OR ALTER PROCEDURE dbo.USp_FIN_DetOptionalGroupFeeHeadMapping
    @GroupCode     VARCHAR(10),
    @BranchCode    VARCHAR(10),
    @OptFeeGroupId INT,
    @ClassCode     VARCHAR(20),
    @TillDate      DATE,
    @CreatedBy     VARCHAR(50),
    @FeeHeads      dbo.FIN_OptionalGroupFeeHeadType READONLY,
    @Status        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Status = -99;

    IF NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_MstOptionalFeeGroup
                    WHERE SeedId = @OptFeeGroupId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND IsValid = 1)
    BEGIN
        SET @Status = -1; RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM @FeeHeads)
    BEGIN
        SET @Status = -2; RETURN;
    END

    IF EXISTS (SELECT 1 FROM @FeeHeads WHERE FeeHeadId <= 0 OR Amount < 0)
    BEGIN
        SET @Status = -3; RETURN;
    END

    IF @TillDate IS NULL OR LTRIM(RTRIM(ISNULL(@ClassCode, ''))) = ''
    BEGIN
        SET @Status = -4; RETURN;
    END

    DECLARE @HadActive BIT =
        CASE WHEN EXISTS (SELECT 1 FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping
                           WHERE OptFeeGroupId = @OptFeeGroupId AND ClassCode = @ClassCode AND IsValid = 1)
             THEN 1 ELSE 0 END;

    BEGIN TRY
        BEGIN TRAN;

        -- heads taken off the class
        UPDATE m
           SET m.IsValid = 0, m.UpdatedBy = @CreatedBy, m.UpdatedDate = GETDATE()
          FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping AS m
         WHERE m.OptFeeGroupId = @OptFeeGroupId
           AND m.ClassCode     = @ClassCode
           AND m.IsValid       = 1
           AND NOT EXISTS (SELECT 1 FROM @FeeHeads AS f WHERE f.FeeHeadId = m.FeeHeadId);

        -- heads already there (active or not): amount / till date
        UPDATE m
           SET m.Amount = f.Amount, m.TillDate = @TillDate, m.IsValid = 1,
               m.UpdatedBy = @CreatedBy, m.UpdatedDate = GETDATE()
          FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping AS m
          INNER JOIN @FeeHeads AS f ON f.FeeHeadId = m.FeeHeadId
         WHERE m.OptFeeGroupId = @OptFeeGroupId
           AND m.ClassCode     = @ClassCode;

        -- new heads
        INSERT INTO dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping
               (OptFeeGroupId, ClassCode, FeeHeadId, Amount, TillDate, IsValid, CreatedBy, CreatedDate)
        SELECT @OptFeeGroupId, @ClassCode, f.FeeHeadId, f.Amount, @TillDate, 1, @CreatedBy, GETDATE()
          FROM @FeeHeads AS f
         WHERE NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping AS m
                            WHERE m.OptFeeGroupId = @OptFeeGroupId
                              AND m.ClassCode     = @ClassCode
                              AND m.FeeHeadId     = f.FeeHeadId);

        COMMIT TRAN;
        SET @Status = CASE WHEN @HadActive = 1 THEN 2 ELSE 1 END;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SET @Status = -99;
        THROW;
    END CATCH
END
GO

/* ---------- 4) unmap one class ----------
   @Status 1 unmapped, 0 class was not mapped, -1 group not found in this branch */
CREATE OR ALTER PROCEDURE dbo.USp_FIN_UnmapOptionalGroupClass
    @GroupCode     VARCHAR(10),
    @BranchCode    VARCHAR(10),
    @OptFeeGroupId INT,
    @ClassCode     VARCHAR(20),
    @UpdatedBy     VARCHAR(50),
    @Status        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_MstOptionalFeeGroup
                    WHERE SeedId = @OptFeeGroupId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
    BEGIN
        SET @Status = -1; RETURN;
    END

    UPDATE dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping
       SET IsValid = 0, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
     WHERE OptFeeGroupId = @OptFeeGroupId
       AND ClassCode     = @ClassCode
       AND IsValid       = 1;

    SET @Status = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
END
GO

/* ---------- 5) mapped classes / heads of one group (popup opens with these checked) ---------- */
CREATE OR ALTER PROCEDURE dbo.USp_FIN_GetOptionalGroupFeeHeadMapping
    @GroupCode     VARCHAR(10),
    @BranchCode    VARCHAR(10),
    @OptFeeGroupId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.ClassCode,
           m.FeeHeadId,
           m.Amount,
           m.TillDate
      FROM dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping AS m
      INNER JOIN dbo.V3M_FIN_MstOptionalFeeGroup AS g ON g.SeedId = m.OptFeeGroupId
     WHERE m.OptFeeGroupId = @OptFeeGroupId
       AND g.GroupCode     = @GroupCode
       AND g.BranchCode    = @BranchCode
       AND m.IsValid       = 1
     ORDER BY m.ClassCode, m.FeeHeadId;
END
GO

/* ---------- 6) index: every call above filters on group + class ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE name = 'IX_FIN_DetOptGroupFeeHeadMap_GroupClass'
                  AND object_id = OBJECT_ID('dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping'))
    CREATE NONCLUSTERED INDEX IX_FIN_DetOptGroupFeeHeadMap_GroupClass
        ON dbo.V3M_FIN_DetOptionalGroupFeeHeadMapping (OptFeeGroupId, ClassCode)
        INCLUDE (FeeHeadId, Amount, TillDate, IsValid);
GO
