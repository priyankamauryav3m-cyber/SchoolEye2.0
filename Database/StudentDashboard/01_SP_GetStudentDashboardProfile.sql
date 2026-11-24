/* =====================================================================================
   Student Dashboard - profile of the LOGGED-IN student only
   Used by : api/StudentDashboard (ServerWebAPI) -> StudentDashboard.razor (/StudentDashboard)

   Security:
     - Takes only @UserSid, which the API reads from the verified JWT (never from the client),
       so a student can never open another student's record.
     - Student is found through MstUsers.UserName = StudentMaster.LoginName (same Group + Branch).
       Old data has a few LoginNames shared by several students; for those, the student created
       at the same time as the user row is chosen (admission SP creates both together).
     - Only display fields are returned: NO Aadhaar, bank, income, password or token columns.

   Change type : NEW object only (read-only SELECT). No table / existing SP is changed.
   Rollback    : DROP PROCEDURE dbo.SP_GetStudentDashboardProfile;
   ===================================================================================== */
CREATE OR ALTER PROCEDURE dbo.SP_GetStudentDashboardProfile
(
    @UserSid INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserName    VARCHAR(50),
            @GroupCode   VARCHAR(20),
            @BranchCode  VARCHAR(20),
            @UserCreated DATETIME,
            @StudentId   BIGINT;

    SELECT @UserName    = mu.UserName,
           @GroupCode   = mu.GroupCode,
           @BranchCode  = mu.BranchCode,
           @UserCreated = mu.CreatedDate
    FROM MstUsers mu WITH (NOLOCK)
    WHERE mu.UserSid = @UserSid
      AND mu.IsValid = 1;

    IF @UserName IS NULL
        RETURN;   -- no row -> API answers "Student record was not found."

    SELECT TOP 1 @StudentId = sm.StudentId
    FROM StudentMaster sm WITH (NOLOCK)
    WHERE sm.LoginName COLLATE DATABASE_DEFAULT = @UserName COLLATE DATABASE_DEFAULT
      AND sm.GroupCode  = @GroupCode
      AND sm.BranchCode = @BranchCode
    ORDER BY
        CASE WHEN ABS(DATEDIFF(SECOND, sm.CreatedDate, @UserCreated)) <= 5 THEN 0 ELSE 1 END,
        ISNULL(sm.IsValid, 0) DESC,
        sm.StudentId DESC;

    IF @StudentId IS NULL
        RETURN;

    SELECT
        sm.StudentId,
        dbo.Fn_GetStudentName(sm.GroupCode, sm.BranchCode, sm.StudentId) AS StudentName,
        sm.FirstName,
        sm.Gender,
        sm.DateOfBirth,
        sm.BloodGroup,
        sm.ImagePath,
        sm.AdmissionNo,
        sm.StudentNo,
        sm.ControlNo,
        NULLIF(sm.RollNo, 0)                          AS RollNo,
        sm.AdmissionDate,
        sm.ApaarId,
        sm.PenNo,
        sm.EmailId                                    AS StudentEmail,
        sm.MobileNo                                   AS StudentMobile,
        CAST(ISNULL(sm.IsValid, 0) AS BIT)            AS IsActive,
        CAST(ISNULL(sm.IsTransportRequired, 0) AS BIT) AS IsTransportRequired,
        mbc.ClassName,
        mcs.SectionName,
        ses.SessionName,
        hs.HouseName,
        hs.HouseColor,
        rel.ReligionName,
        cat.CategoryName,
        dbo.Fn_GetStudentRouteName(sm.GroupCode, sm.BranchCode, sm.StudentId, 'PickUp') AS RouteName,
        br.BranchName,
        grp.GroupName,
        -- parents (names + contact only)
        LTRIM(RTRIM(CONCAT(sp.FatherName, ' ', sp.FatherMName, ' ', sp.FatherLName))) AS FatherName,
        sp.FatherContactNo,
        sp.FatherEMail                                AS FatherEmail,
        sp.FatherImagePath,
        LTRIM(RTRIM(CONCAT(sp.MotherName, ' ', sp.MotherMName, ' ', sp.MotherLName))) AS MotherName,
        sp.MotherContactNo,
        sp.MotherEMail                                AS MotherEmail,
        sp.MotherImagePath,
        sp.EmergencyPersonName,
        sp.EmergencyPersonRelationShip,
        sp.EmergencyPersonContactNo,
        -- address: current first, else permanent
        adr.Line1                                     AS AddressLine1,
        adr.Line2                                     AS AddressLine2,
        adr.PinCode,
        dis.DistrictName,
        st.StateName
    FROM StudentMaster sm WITH (NOLOCK)
    LEFT JOIN MstBranchClass mbc WITH (NOLOCK)
           ON mbc.ClassCode = sm.ClassCode AND mbc.GroupCode = sm.GroupCode AND mbc.BranchCode = sm.BranchCode
    LEFT JOIN MstClassSection mcs WITH (NOLOCK)
           ON mcs.SectionId = sm.SectionId AND mcs.GroupCode = sm.GroupCode AND mcs.BranchCode = sm.BranchCode
    LEFT JOIN MstBranchSession ses WITH (NOLOCK)
           ON ses.SessionId = sm.SessionId AND ses.GroupCode = sm.GroupCode AND ses.BranchCode = sm.BranchCode
    LEFT JOIN MstSchoolHouse hs WITH (NOLOCK)
           ON hs.HouseID = sm.HouseNo AND hs.GroupCode = sm.GroupCode AND hs.BranchCode = sm.BranchCode
    LEFT JOIN MstReligion rel WITH (NOLOCK) ON rel.ReligionId = sm.Religion
    LEFT JOIN MstSocialCategory cat WITH (NOLOCK) ON cat.CategoryId = sm.SocialCategory
    OUTER APPLY (SELECT TOP 1 b.BranchName FROM MstBranchMaster b WITH (NOLOCK)
                 WHERE b.GroupCode = sm.GroupCode AND b.BranchCode = sm.BranchCode) br
    OUTER APPLY (SELECT TOP 1 g.GroupName FROM MstGroupMaster g WITH (NOLOCK)
                 WHERE g.GroupCode = sm.GroupCode) grp
    OUTER APPLY (SELECT TOP 1 p.FatherName, p.FatherMName, p.FatherLName, p.FatherContactNo, p.FatherEMail,
                        p.FatherImagePath,
                        p.MotherName, p.MotherMName, p.MotherLName, p.MotherContactNo, p.MotherEMail,
                        p.MotherImagePath,
                        p.EmergencyPersonName, p.EmergencyPersonRelationShip, p.EmergencyPersonContactNo
                 FROM StudentParent p WITH (NOLOCK)
                 WHERE p.GroupCode = sm.GroupCode AND p.BranchCode = sm.BranchCode AND p.StudentId = sm.StudentId) sp
    OUTER APPLY (SELECT TOP 1 a.Line1, a.Line2, a.PinCode, a.DistrictId, a.StateId
                 FROM StudentAddress a WITH (NOLOCK)
                 WHERE a.GroupCode = sm.GroupCode AND a.BranchCode = sm.BranchCode AND a.StudentId = sm.StudentId
                 ORDER BY CASE a.AddressType WHEN 'C' THEN 0 ELSE 1 END) adr
    LEFT JOIN MstDistrict dis WITH (NOLOCK) ON dis.DistrictId = adr.DistrictId
    LEFT JOIN MstState st WITH (NOLOCK) ON st.StateId = adr.StateId
    WHERE sm.StudentId = @StudentId;
END
GO

-- Test: EXEC dbo.SP_GetStudentDashboardProfile @UserSid = 22074;
