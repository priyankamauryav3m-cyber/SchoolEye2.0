/* ============================================================================
   USP_AddUpdateDynamicFormController2 - same field name in more than one activity
   Before: a field name could be in only ONE activity of a branch (-4).
   Now   : the duplicate check is per ACTIVITY (GroupCode + BranchCode + SAId + FieldName),
           so "StudentName" can be added to Swimming AND Badminton; both use the same
           OtherActivities.StudentName column and the field shows on both forms.
   A column is shared by every activity / branch that uses the name, so:
     -7  insert: the column already exists with another data type (pick the same data type)
     -8  update: the data type cannot be changed, the column is used by another field
   A longer max length widens the shared nvarchar column; it is never made shorter
   while another field uses it.
   Other results as before: 1 inserted, 2 updated, -1 not found, -2 invalid name,
   -3 name required, -4 already in this activity, -99 error.
   Rollback: run the previous body of the procedure (sp_helptext before running this).
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_AddUpdateDynamicFormController2]
(
    @GroupCode VARCHAR(50), @BranchCode VARCHAR(50), @DFCId INT = 0, @FieldName NVARCHAR(100), @DisplayName NVARCHAR(100), @DisplayOrder INT = NULL,
    @MaxLength INT = NULL, @FieldType NVARCHAR(50), @DataType NVARCHAR(50), @OptionValues NVARCHAR(MAX) = NULL, @IsMandatory BIT = 0, @CreatedBy VARCHAR(50),
    @SAId INT, @Result INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FieldName IS NULL OR LTRIM(RTRIM(@FieldName)) = ''
        BEGIN
            SET @Result = -3; -- FieldName required
            RETURN;
        END
        IF @FieldName LIKE '%[^a-zA-Z0-9_]%'
        BEGIN
            SET @Result = -2; -- Invalid column name
            RETURN;
        END

        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @QuotedField NVARCHAR(150) = QUOTENAME(@FieldName);
        DECLARE @Len INT = CASE WHEN @MaxLength IS NULL OR @MaxLength <= 0 THEN 100 ELSE @MaxLength END;

        -- column type of the data type, and its base type (to compare with an existing column)
        DECLARE @BaseType SYSNAME =
            CASE LOWER(@DataType)
                WHEN 'string'   THEN 'nvarchar'
                WHEN 'int'      THEN 'int'
                WHEN 'long'     THEN 'bigint'
                WHEN 'decimal'  THEN 'decimal'
                WHEN 'double'   THEN 'float'
                WHEN 'datetime' THEN 'datetime2'
                WHEN 'bit'      THEN 'bit'
                ELSE 'nvarchar'
            END;
        DECLARE @ColumnType NVARCHAR(100) =
            CASE @BaseType
                WHEN 'nvarchar' THEN 'NVARCHAR(' + CAST(@Len AS VARCHAR(10)) + ')'
                WHEN 'int'      THEN 'INT'
                WHEN 'bigint'   THEN 'BIGINT'
                WHEN 'decimal'  THEN 'DECIMAL(10,2)'
                WHEN 'float'    THEN 'FLOAT'
                WHEN 'datetime2' THEN 'DATETIME2'
                WHEN 'bit'      THEN 'BIT'
            END;

        -- the OtherActivities column of this name (shared by every field with this name)
        DECLARE @ExistType SYSNAME, @ExistLen INT;
        -- old (pre-made) columns of the table, e.g. StudentName / Class / DateOfBirth, map the same way:
        -- varchar / char -> text, datetime / date -> datetime
        SELECT @ExistType = CASE WHEN TYPE_NAME(c.system_type_id) IN ('nvarchar','varchar','nchar','char') THEN 'nvarchar'
                                 WHEN TYPE_NAME(c.system_type_id) IN ('datetime','datetime2','date','smalldatetime') THEN 'datetime2'
                                 ELSE TYPE_NAME(c.system_type_id) END,
               @ExistLen  = CASE WHEN c.max_length = -1 THEN -1
                                 WHEN TYPE_NAME(c.system_type_id) IN ('nvarchar','nchar') THEN c.max_length / 2
                                 ELSE c.max_length END
        FROM sys.columns c
        WHERE c.object_id = OBJECT_ID(N'dbo.OtherActivities') AND c.name = @FieldName;

        BEGIN TRAN;

        IF @DFCId > 0
        BEGIN
            ---------------------------------------------------
            -- UPDATE
            ---------------------------------------------------
            IF NOT EXISTS (SELECT 1 FROM DynamicFormController WHERE DFCId = @DFCId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
            BEGIN
                SET @Result = -1; -- Record not found
                COMMIT TRAN;
                RETURN;
            END

            -- another activity in this branch already has this name for the new activity
            IF EXISTS (SELECT 1 FROM DynamicFormController
                       WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SAId = @SAId
                         AND FieldName = @FieldName AND IsValid = 1 AND DFCId <> @DFCId)
            BEGIN
                SET @Result = -4;
                COMMIT TRAN;
                RETURN;
            END

            DECLARE @Shared BIT = CASE WHEN EXISTS (SELECT 1 FROM DynamicFormController
                                                    WHERE FieldName = @FieldName AND DFCId <> @DFCId AND IsValid = 1)
                                       THEN 1 ELSE 0 END;

            IF @Shared = 1 AND @ExistType IS NOT NULL AND @ExistType <> @BaseType
            BEGIN
                SET @Result = -8; -- data type of a shared column cannot change
                COMMIT TRAN;
                RETURN;
            END

            UPDATE DynamicFormController
               SET DisplayName = @DisplayName, DisplayOrder = @DisplayOrder, MaxLength = @MaxLength,
                   FieldType = @FieldType, DataType = @DataType, OptionValues = @OptionValues,
                   IsMandatory = @IsMandatory, CreatedDate = GETDATE(), CreatedBy = @CreatedBy, SAId = @SAId
             WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND DFCId = @DFCId;

            IF @ExistType IS NOT NULL
               AND (   (@Shared = 0 AND @ExistType <> @BaseType)
                    OR (@BaseType = 'nvarchar' AND @ExistType = 'nvarchar' AND @ExistLen <> -1 AND @Len > @ExistLen))
            BEGIN
                -- own column: data type follows the field; a text column is only made longer, never shorter
                -- (old saved data is never cut)
                SET @SQL = N'ALTER TABLE dbo.OtherActivities ALTER COLUMN ' + @QuotedField + N' ' + @ColumnType;
                EXEC sp_executesql @SQL;
            END

            SET @Result = 2; -- Updated
        END
        ELSE
        BEGIN
            ---------------------------------------------------
            -- INSERT
            ---------------------------------------------------
            -- duplicate only inside the same activity
            IF EXISTS (SELECT 1 FROM DynamicFormController
                       WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SAId = @SAId
                         AND FieldName = @FieldName AND IsValid = 1)
            BEGIN
                SET @Result = -4; -- Field already exists in this activity
                COMMIT TRAN;
                RETURN;
            END

            -- the column exists (another activity / branch): same data type only
            IF @ExistType IS NOT NULL AND @ExistType <> @BaseType
            BEGIN
                SET @Result = -7;
                COMMIT TRAN;
                RETURN;
            END

            INSERT INTO DynamicFormController (GroupCode, BranchCode, FieldName, DisplayName, DisplayOrder, MaxLength, FieldType, DataType, OptionValues, IsMandatory, IsValid, CreatedDate, CreatedBy, SAId)
            VALUES (@GroupCode, @BranchCode, @FieldName, @DisplayName, @DisplayOrder, @MaxLength, @FieldType, @DataType, @OptionValues, @IsMandatory, 1, GETDATE(), @CreatedBy, @SAId);

            IF @ExistType IS NULL
            BEGIN
                SET @SQL = N'ALTER TABLE dbo.OtherActivities ADD ' + @QuotedField + N' ' + @ColumnType;
                EXEC sp_executesql @SQL;
            END
            ELSE IF @BaseType = 'nvarchar' AND @ExistLen <> -1 AND @Len > @ExistLen
            BEGIN
                -- shared text column too short for this field: make it longer
                SET @SQL = N'ALTER TABLE dbo.OtherActivities ALTER COLUMN ' + @QuotedField + N' ' + @ColumnType;
                EXEC sp_executesql @SQL;
            END

            SET @Result = 1; -- Inserted
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;
        SET @Result = -99;
    END CATCH
END
GO
