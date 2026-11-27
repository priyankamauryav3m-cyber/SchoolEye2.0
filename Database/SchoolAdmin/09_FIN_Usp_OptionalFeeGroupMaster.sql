/* ============================================================================
   Optional Fee Group master (/OptionalFeeGroupMaster)
   Table: V3M_FIN_MstOptionalFeeGroup (SeedId, GroupCode, BranchCode, ActivityGroupName, IsValid)
   Save stays on the existing FIN_USp_OptionalFeeGroup (not changed here).

   1) FIN_Usp_GetOptionalFeeGroupList
      replaces the old inline query (select * ... like '%name%' ... IsValid = mode)
      @ActivityGroupName  '' = all, otherwise part of the name
      @Valid              1 active, 0 deactive, -1 all

   2) FIN_Usp_ActiveDeactiveOptionalFeeGroup
      @IsValid 1 = activate, 0 = deactivate
      @Result  1 = changed, -1 = not found in this group / branch

   Rollback:
      DROP PROCEDURE dbo.FIN_Usp_GetOptionalFeeGroupList;
      DROP PROCEDURE dbo.FIN_Usp_ActiveDeactiveOptionalFeeGroup;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.FIN_Usp_GetOptionalFeeGroupList
    @GroupCode         VARCHAR(50),
    @BranchCode        VARCHAR(50),
    @ActivityGroupName NVARCHAR(200) = '',
    @Valid             INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    SET @ActivityGroupName = LTRIM(RTRIM(ISNULL(@ActivityGroupName, '')));

    SELECT og.SeedId,
           og.GroupCode,
           og.BranchCode,
           og.ActivityGroupName,
           CAST(ISNULL(og.IsValid, 0) AS BIT) AS IsValid
      FROM V3M_FIN_MstOptionalFeeGroup AS og
     WHERE og.GroupCode  = @GroupCode
       AND og.BranchCode = @BranchCode
       AND (@ActivityGroupName = '' OR og.ActivityGroupName LIKE '%' + @ActivityGroupName + '%')
       AND (@Valid = -1 OR og.IsValid = @Valid)
     ORDER BY og.ActivityGroupName;
END
GO

CREATE OR ALTER PROCEDURE dbo.FIN_Usp_ActiveDeactiveOptionalFeeGroup
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @SeedId     INT,
    @IsValid    BIT,
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE V3M_FIN_MstOptionalFeeGroup
       SET IsValid = @IsValid
     WHERE SeedId     = @SeedId
       AND GroupCode  = @GroupCode
       AND BranchCode = @BranchCode;

    SET @Result = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE -1 END;
END
GO
