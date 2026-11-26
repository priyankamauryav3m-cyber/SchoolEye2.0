/* ============================================================================
   Summer Activity Slot master - active / deactive of one slot
   Table: SummerActivitySlots (SASId identity, IsValid bit); group / branch come
   from the slot's activity (SummerActivities).
   @IsValid 1 = activate, 0 = deactivate
   @Result   1 = changed, -1 = not found in this group / branch
   Rollback: DROP PROCEDURE dbo.Usp_ActiveDeactiveSummerActivitySlot;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_ActiveDeactiveSummerActivitySlot
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @SASId      INT,
    @IsValid    BIT,
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE sas
       SET sas.IsValid = @IsValid
      FROM SummerActivitySlots AS sas
      INNER JOIN SummerActivities AS sa ON sa.SAId = sas.SAId
     WHERE sas.SASId     = @SASId
       AND sa.GroupCode  = @GroupCode
       AND sa.BranchCode = @BranchCode;

    SET @Result = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE -1 END;
END
GO
