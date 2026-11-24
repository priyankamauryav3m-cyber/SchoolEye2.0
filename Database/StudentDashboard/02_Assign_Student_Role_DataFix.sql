/* =====================================================================================
   DATA FIX (review before running - NOT run automatically)

   Problem : Admission SPs create the student's login in MstUsers but put the role only in
             MstUsersRights; MstUsers.RoleId stays NULL. SP_AuthenticateUserLogin does an
             INNER JOIN on MstRoles via MstUsers.RoleId, so these students cannot log in.
   Fix     : Set RoleId = Student role for MstUsers rows that are linked to a student
             (UserName = StudentMaster.LoginName, same Group + Branch) and have no role yet.
             Rows that already have a role are NOT touched.

   Run steps:
     1. Run STEP 1 (preview) and check the count.
     2. Run STEP 2 (backup + update) in one go.
     3. Rollback, if needed: STEP 3.

   Note: new admissions will again get RoleId NULL until the admission SPs
         (ADM_UspStudentDirectAdmission / V3M_ADM_UspAdmitChild / V3M_FIN_UspAdmitChild ...)
         also set MstUsers.RoleId. That is a separate change (approval needed).
   ===================================================================================== */

DECLARE @StudentRoleId INT = (SELECT TOP 1 RoleId FROM MstRoles WHERE RoleName = 'Student' AND ISNULL(IsValid, 1) = 1);
SELECT @StudentRoleId AS StudentRoleId;   -- expected 5

/* ---------- STEP 1: preview ---------- */
SELECT COUNT(*) AS UsersToUpdate
FROM MstUsers mu
WHERE mu.RoleId IS NULL
  AND EXISTS (SELECT 1 FROM StudentMaster sm
              WHERE sm.LoginName COLLATE DATABASE_DEFAULT = mu.UserName COLLATE DATABASE_DEFAULT
                AND sm.GroupCode = mu.GroupCode AND sm.BranchCode = mu.BranchCode);

/* ---------- STEP 2: backup + update ---------- */
/*
IF OBJECT_ID('dbo.Bkp_MstUsers_StudentRole_20261124') IS NULL
    SELECT mu.UserSid, mu.RoleId
    INTO dbo.Bkp_MstUsers_StudentRole_20261124
    FROM MstUsers mu
    WHERE mu.RoleId IS NULL
      AND EXISTS (SELECT 1 FROM StudentMaster sm
                  WHERE sm.LoginName COLLATE DATABASE_DEFAULT = mu.UserName COLLATE DATABASE_DEFAULT
                    AND sm.GroupCode = mu.GroupCode AND sm.BranchCode = mu.BranchCode);

BEGIN TRAN;
    UPDATE mu
       SET mu.RoleId = @StudentRoleId
    FROM MstUsers mu
    INNER JOIN dbo.Bkp_MstUsers_StudentRole_20261124 b ON b.UserSid = mu.UserSid
    WHERE mu.RoleId IS NULL;
    SELECT @@ROWCOUNT AS UsersUpdated;
COMMIT TRAN;
*/

/* ---------- STEP 3: rollback ---------- */
/*
UPDATE mu SET mu.RoleId = NULL
FROM MstUsers mu
INNER JOIN dbo.Bkp_MstUsers_StudentRole_20261124 b ON b.UserSid = mu.UserSid;
*/
