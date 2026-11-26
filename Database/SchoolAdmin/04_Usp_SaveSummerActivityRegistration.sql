/* ============================================================================
   Summer activity registration (public page from the tiny link) -> OtherActivities
   One row per registration:
     - Activity / SlotName / Amount come from the DB (active activity of the
       group / branch / session, active slot of that activity), never from the page.
     - @FieldValues: JSON [{"FieldName":"StudentName","Value":"..."}, ...]
       Only columns that are ACTIVE fields of this activity in DynamicFormController
       AND exist in OtherActivities are written (base columns such as Amount,
       IsFeePaid can never be set from the page). Values are passed as a parameter
       (sp_executesql), column names with QUOTENAME.
   @Result  1 saved (@OAId = new row), -1 activity not found / not active,
           -2 registration closed (end date over), -3 slot not available,
           -4 mandatory field missing, -6 invalid values JSON, -99 not saved
   IsFeePaid = 0 (payment is not part of this step).
   Rollback: DROP PROCEDURE dbo.Usp_SaveSummerActivityRegistration;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_SaveSummerActivityRegistration
    @GroupCode   VARCHAR(50),
    @BranchCode  VARCHAR(50),
    @SessionId   BIGINT,
    @SAId        INT,
    @SASId       INT,
    @FieldValues NVARCHAR(MAX),
    @CreatedBy   VARCHAR(50),
    @OAId        INT OUTPUT,
    @Result      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @OAId = NULL;

    IF ISJSON(ISNULL(@FieldValues, N'[]')) <> 1
    BEGIN
        SET @Result = -6;
        RETURN;
    END

    DECLARE @EndDate DATETIME;
    SELECT @EndDate = EndDate
    FROM SummerActivities
    WHERE SAId = @SAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode
      AND SessionId = @SessionId AND IsValid = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        SET @Result = -1;
        RETURN;
    END

    IF @EndDate IS NOT NULL AND CAST(@EndDate AS DATE) < CAST(GETDATE() AS DATE)
    BEGIN
        SET @Result = -2;
        RETURN;
    END

    DECLARE @SlotName NVARCHAR(200), @Amount DECIMAL(10, 2);
    SELECT @SlotName = SlotName, @Amount = FeeAmount
    FROM SummerActivitySlots
    WHERE SASId = @SASId AND SAId = @SAId AND IsValid = 1;

    IF @SlotName IS NULL
    BEGIN
        SET @Result = -3;
        RETURN;
    END

    DECLARE @Values TABLE (FieldName NVARCHAR(200), Value NVARCHAR(MAX));
    INSERT INTO @Values (FieldName, Value)
    SELECT FieldName, NULLIF(LTRIM(RTRIM(Value)), N'')
    FROM OPENJSON(ISNULL(@FieldValues, N'[]'))
         WITH (FieldName NVARCHAR(200) '$.FieldName', Value NVARCHAR(MAX) '$.Value');

    -- every mandatory active field must have a value
    IF EXISTS (
        SELECT 1
        FROM DynamicFormController d
        WHERE d.GroupCode = @GroupCode AND d.BranchCode = @BranchCode AND d.SAId = @SAId
          AND d.IsValid = 1 AND d.IsMandatory = 1
          AND NOT EXISTS (SELECT 1 FROM @Values v
                          WHERE v.FieldName COLLATE DATABASE_DEFAULT = d.FieldName COLLATE DATABASE_DEFAULT
                            AND v.Value IS NOT NULL))
    BEGIN
        SET @Result = -4;
        RETURN;
    END

    -- SET list: only active fields of this activity that are columns of OtherActivities
    DECLARE @Set NVARCHAR(MAX), @SQL NVARCHAR(MAX);
    SELECT @Set = STRING_AGG(CAST(
               QUOTENAME(c.name) + N' = (SELECT TOP (1) j.Value FROM OPENJSON(@json) WITH (FieldName NVARCHAR(200) ''$.FieldName'', Value NVARCHAR(MAX) ''$.Value'') j WHERE j.FieldName = '
               + QUOTENAME(c.name, N'''') + N')'
           AS NVARCHAR(MAX)), N', ')
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.OtherActivities')
      AND EXISTS (SELECT 1 FROM DynamicFormController d
                  WHERE d.GroupCode = @GroupCode AND d.BranchCode = @BranchCode AND d.SAId = @SAId AND d.IsValid = 1
                    AND d.FieldName COLLATE DATABASE_DEFAULT = c.name COLLATE DATABASE_DEFAULT)
      AND EXISTS (SELECT 1 FROM @Values v
                  WHERE v.Value IS NOT NULL
                    AND v.FieldName COLLATE DATABASE_DEFAULT = c.name COLLATE DATABASE_DEFAULT);

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO OtherActivities (GroupCode, BranchCode, Activity, SlotName, Amount, IsValid, IsFeePaid, CreatedDate, CreatedBy)
        VALUES (@GroupCode, @BranchCode, @SAId, @SlotName, @Amount, 1, 0, GETDATE(), @CreatedBy);

        SET @OAId = CAST(SCOPE_IDENTITY() AS INT);

        IF @Set IS NOT NULL
        BEGIN
            SET @SQL = N'UPDATE dbo.OtherActivities SET ' + @Set + N' WHERE OAId = @OAId;';
            EXEC sp_executesql @SQL, N'@json NVARCHAR(MAX), @OAId INT', @json = @FieldValues, @OAId = @OAId;
        END

        COMMIT TRAN;
        SET @Result = 1;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;
        SET @OAId = NULL;
        SET @Result = -99;
    END CATCH
END
GO
