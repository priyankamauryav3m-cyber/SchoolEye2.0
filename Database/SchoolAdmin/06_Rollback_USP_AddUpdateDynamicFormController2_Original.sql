-- ROLLBACK: original body of USP_AddUpdateDynamicFormController2 (saved from live before 06). Run this file to go back.
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

        ---------------------------------------------------
        -- BASIC VALIDATION
        ---------------------------------------------------
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

        ---------------------------------------------------
        -- DECLARE VARIABLES
        ---------------------------------------------------
        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @ColumnType NVARCHAR(100);
        DECLARE @QuotedField NVARCHAR(150) = QUOTENAME(@FieldName);

        ---------------------------------------------------
        -- SET DYNAMIC SQL DATATYPE
        -- ?? FIX: DateTime branch ab LOWER() se compare hoti hai (pehle kabhi match nahi hoti thi)
        ---------------------------------------------------
        SET @ColumnType =
            CASE
                WHEN LOWER(@DataType) = 'string'
                    THEN
                        CASE
                            WHEN @MaxLength IS NULL OR @MaxLength <= 0
                                THEN 'NVARCHAR(100)'
                            ELSE 'NVARCHAR(' + CAST(@MaxLength AS VARCHAR(10)) + ')'
                        END

                WHEN LOWER(@DataType) = 'int'
                    THEN 'INT'

                WHEN LOWER(@DataType) = 'long'
                    THEN 'BIGINT'

                WHEN LOWER(@DataType) = 'decimal'
                    THEN 'DECIMAL(10,2)'

                WHEN LOWER(@DataType) = 'double'
                    THEN 'FLOAT'

                WHEN LOWER(@DataType) = 'datetime'
                    THEN 'DATETIME2'

                WHEN LOWER(@DataType) = 'bit'
                    THEN 'BIT'

                ELSE 'NVARCHAR(100)'
            END;

        BEGIN TRAN;

        ---------------------------------------------------
        -- UPDATE BLOCK
        ---------------------------------------------------
        IF @DFCId > 0
        BEGIN

            IF EXISTS ( SELECT 1 FROM DynamicFormController WHERE DFCId = @DFCId AND GroupCode = @GroupCode AND BranchCode = @BranchCode )
            BEGIN

                ---------------------------------------------------
                -- UPDATE CONFIGURATION TABLE
                ---------------------------------------------------
                UPDATE DynamicFormController
                SET DisplayName = @DisplayName, DisplayOrder = @DisplayOrder, MaxLength = @MaxLength,
                    FieldType = @FieldType, DataType = @DataType, OptionValues = @OptionValues,
                    IsMandatory = @IsMandatory, CreatedDate = GETDATE(), CreatedBy = @CreatedBy, SAId = @SAId 
                WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND DFCId = @DFCId;

                ---------------------------------------------------
                -- ALTER COLUMN IF EXISTS (QUOTENAME se safe)
                ---------------------------------------------------
                IF EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OtherActivities' AND COLUMN_NAME = @FieldName )
                BEGIN
                    SET @SQL = N'ALTER TABLE dbo.OtherActivities ALTER COLUMN ' + @QuotedField + N' ' + @ColumnType;
                    EXEC sp_executesql @SQL;
                END

                SET @Result = 2; -- Updated

            END
            ELSE
            BEGIN
                SET @Result = -1; -- Record not found
            END

        END

        ---------------------------------------------------
        -- INSERT BLOCK
        ---------------------------------------------------
        ELSE
        BEGIN

            ---------------------------------------------------
            -- DUPLICATE CHECK (naya) - same GroupCode+BranchCode+FieldName
            -- dobara metadata-row banne se roko
            ---------------------------------------------------
            IF EXISTS ( SELECT 1 FROM DynamicFormController WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND FieldName = @FieldName AND IsValid = 1 )
            BEGIN
                SET @Result = -4; -- Field already exists for this GroupCode/BranchCode
            END
            ELSE
            BEGIN

                ---------------------------------------------------
                -- INSERT CONFIGURATION
                ---------------------------------------------------
                INSERT INTO DynamicFormController ( GroupCode, BranchCode, FieldName, DisplayName, DisplayOrder, MaxLength, FieldType, DataType, OptionValues, IsMandatory, IsValid, CreatedDate, CreatedBy, SAId )
                VALUES(@GroupCode, @BranchCode, @FieldName, @DisplayName, @DisplayOrder, @MaxLength, @FieldType, @DataType, @OptionValues, @IsMandatory, 1, GETDATE(), @CreatedBy, @SAId);

                ---------------------------------------------------
                -- ADD COLUMN IF NOT EXISTS (QUOTENAME se safe)
                ---------------------------------------------------
                IF NOT EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OtherActivities' AND COLUMN_NAME = @FieldName )
                BEGIN
                    SET @SQL = N'ALTER TABLE dbo.OtherActivities ADD ' + @QuotedField + N' ' + @ColumnType;
                    EXEC sp_executesql @SQL;
                END

                SET @Result = 1; -- Inserted

            END

        END

        COMMIT TRAN;

    END TRY

    ---------------------------------------------------
    -- ERROR HANDLING
    ---------------------------------------------------
    BEGIN CATCH

        IF XACT_STATE() <> 0
            ROLLBACK TRAN;

        SET @Result = -99;

        -- ?? PRINT sirf SSMS window mein dikhta hai, caller (C#/API) ko nahi milta -
        -- isliye asal error kahin persist bhi kar do taaki debug ho sake
        -- (agar aapke paas Det_ExceptionDetails jaisi koi log-table hai, use kar lo)
        -- INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        -- VALUES('DynamicForm','DynamicFormController','USP_AddUpdateDynamicFormController2',ERROR_MESSAGE(),GETDATE());

        PRINT ERROR_MESSAGE();

    END CATCH

END
