/* ============================================================================
   Dynamic Form Controller - active / deactive of one field
   Table: DynamicFormController (DFCId identity, IsValid bit). The OtherActivities
   column of the field is NOT dropped: deactive only hides the field.
   @IsValid 1 = activate, 0 = deactivate
   @Result   1 = changed, -1 = not found in this group / branch,
            -4 = activate refused: the same field name is already active in this activity
   Rollback: DROP PROCEDURE dbo.Usp_ActiveDeactiveDynamicFormController;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_ActiveDeactiveDynamicFormController
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @DFCId      INT,
    @IsValid    BIT,
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @IsValid = 1 AND EXISTS (
        SELECT 1
        FROM DynamicFormController AS other
        INNER JOIN DynamicFormController AS me
            ON me.DFCId = @DFCId
           AND other.FieldName = me.FieldName
           AND other.GroupCode = me.GroupCode
           AND other.BranchCode = me.BranchCode
           AND other.SAId = me.SAId          -- same name in the same activity only
        WHERE other.DFCId <> @DFCId
          AND other.IsValid = 1
          AND me.GroupCode = @GroupCode
          AND me.BranchCode = @BranchCode)
    BEGIN
        SET @Result = -4;
        RETURN;
    END

    UPDATE DynamicFormController
       SET IsValid = @IsValid
     WHERE DFCId      = @DFCId
       AND GroupCode  = @GroupCode
       AND BranchCode = @BranchCode;

    SET @Result = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE -1 END;
END
GO
