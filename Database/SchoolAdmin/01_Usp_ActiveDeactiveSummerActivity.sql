/* ============================================================================
   Summer Activity master - active / deactive of one activity
   Table: SummerActivities (SAId identity, IsValid bit)
   @IsValid 1 = activate, 0 = deactivate
   @Result   1 = changed, -1 = not found in this group / branch
   Rollback: DROP PROCEDURE dbo.Usp_ActiveDeactiveSummerActivity;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_ActiveDeactiveSummerActivity
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @SAId       INT,
    @IsValid    BIT,
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SummerActivities
       SET IsValid = @IsValid
     WHERE SAId       = @SAId
       AND GroupCode  = @GroupCode
       AND BranchCode = @BranchCode;

    SET @Result = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE -1 END;
END
GO
