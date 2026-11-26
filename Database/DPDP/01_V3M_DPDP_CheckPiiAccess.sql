/* =====================================================================================
   DPDP - Student Sensitive Details
   01. V3M_DPDP_CheckPiiAccess

   Server-side permission check for the Student Sensitive Details page.
   The API calls this for every request (view, history, update); the Blazor menu / role
   in session storage is never trusted.

   Allowed when:
     - the user (UserSid from the verified JWT) is valid,
     - the user's role has IsPII = 1 on the activity whose URL is @URL (Role Master -> PII tick),
     - the user belongs to the requested group, and to the requested branch
       (GroupAdmin / V3MAdmin may work across the branches of their group).

   Returns one row: HasAccess (BIT), UserName (for the audit log "TransBy").
   ===================================================================================== */
CREATE OR ALTER PROCEDURE dbo.V3M_DPDP_CheckPiiAccess
    @UserSid    INT,
    @URL        VARCHAR(200),
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
           CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Mst_ACMapping AM WITH (NOLOCK)
                    INNER JOIN dbo.MstActivityList MA WITH (NOLOCK) ON MA.ActivityId = AM.ActivityId
                    WHERE AM.RoleId = U.RoleId
                      AND ISNULL(AM.IsActive, 1) = 1
                      AND ISNULL(MA.IsValid, 1) = 1
                      AND ISNULL(AM.IsPII, 0) = 1
                      AND LTRIM(RTRIM(MA.URL)) = @URL)
                 AND U.GroupCode = @GroupCode
                 AND (U.BranchCode = @BranchCode OR R.RoleName IN ('GroupAdmin', 'V3MAdmin'))
                THEN 1 ELSE 0 END AS BIT) AS HasAccess,
           U.UserName
    FROM dbo.MstUsers U WITH (NOLOCK)
    LEFT JOIN dbo.MstRoles R WITH (NOLOCK) ON R.RoleId = U.RoleId
    WHERE U.UserSid = @UserSid
      AND ISNULL(U.IsValid, 1) = 1;
END
GO
