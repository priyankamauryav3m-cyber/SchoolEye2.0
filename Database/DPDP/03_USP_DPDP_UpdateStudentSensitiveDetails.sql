/* =====================================================================================
   DPDP - Student Sensitive Details
   03. USP_DPDP_UpdateStudentSensitiveDetails

   The ONLY place where a student's / parent's sensitive details are updated.

   @Changes (existing type dbo.ColumnChangeType) carries only the fields the user changed:
       ColumnName = field key (see 02_V3M_DPDP_GetStudentSensitiveDetails.sql), NewValue = new value
       (OldValue of the input is ignored - the SP reads the real old value itself).

   In ONE transaction:
     1. reads the current (old) values,
     2. updates StudentMaster / StudentParent / StudentAddress / StudentAdditionalInformation
        (only the columns that really changed),
     3. writes the existing *History tables (same as the normal save SPs),
     4. calls dbo.Usp_SaveTransactionLog for every table with OldValue / NewValue
        (ReferenceKey = 'DPDP_StudentSensitive', ReferenceId = StudentId) + the reason.
        The number of rows per field in Det_TransactionLog = how many times it was changed.

   @Result :  1 saved | 3 nothing changed | -1 student not found | -2 unknown field
             -3 reason missing | -4 invalid date / number | 0 error (logged in Det_ExceptionDetails)
   ===================================================================================== */
CREATE OR ALTER PROCEDURE dbo.USP_DPDP_UpdateStudentSensitiveDetails
    @GroupCode    VARCHAR(10),
    @BranchCode   VARCHAR(10),
    @SessionId    BIGINT,
    @StudentId    BIGINT,
    @Changes      dbo.ColumnChangeType READONLY,
    @Reason       NVARCHAR(300),
    @UpdatedBy    VARCHAR(50),
    @IP           VARCHAR(45) = NULL,
    @Result       INT OUTPUT,
    @ChangedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT @Result = 0, @ChangedCount = 0;

    DECLARE @RefKey VARCHAR(100) = 'DPDP_StudentSensitive',
            @RefId  VARCHAR(50)  = CAST(@StudentId AS VARCHAR(50)),
            @LogIP  VARCHAR(20)  = LEFT(@IP, 20),
            @By     VARCHAR(50)  = LEFT(ISNULL(@UpdatedBy, ''), 50);

    ------------------------------------------------------------------
    -- 0. checks
    ------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.StudentMaster WITH (NOLOCK)
                   WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
    BEGIN SET @Result = -1; RETURN; END

    IF NULLIF(LTRIM(RTRIM(@Reason)), '') IS NULL
    BEGIN SET @Result = -3; RETURN; END

    DECLARE @Known TABLE (FieldKey SYSNAME PRIMARY KEY, TableName SYSNAME NOT NULL);
    INSERT INTO @Known (FieldKey, TableName) VALUES
        ('DateOfBirth','StudentMaster'), ('AadhaarNo','StudentMaster'), ('MobileNo','StudentMaster'),
        ('EmailId','StudentMaster'), ('BloodGroup','StudentMaster'), ('MedicalInformation','StudentMaster'),
        ('NatureOfDisability','StudentMaster'),
        ('FatherContactNo','StudentParent'), ('FatherEMail','StudentParent'), ('FatherAadhaarNo','StudentParent'),
        ('FatherDOB','StudentParent'), ('FatherAnnualIncome','StudentParent'), ('FatherOfficeContactNo','StudentParent'),
        ('FatherOfficeAddress','StudentParent'),
        ('MotherContactNo','StudentParent'), ('MotherEMail','StudentParent'), ('MotherAadhaarNo','StudentParent'),
        ('MotherDOB','StudentParent'), ('MotherAnnualIncome','StudentParent'), ('MotherOfficeContactNo','StudentParent'),
        ('MotherOfficeAddress','StudentParent'),
        ('SMSMobileNo','StudentParent'), ('GuardianContactNo','StudentParent'), ('GuardianEmail','StudentParent'),
        ('GuardianAddress','StudentParent'), ('EmergencyPersonContactNo','StudentParent'), ('EmergencyPersonAddress','StudentParent'),
        ('CurrentLine1','StudentAddress'), ('CurrentLine2','StudentAddress'), ('CurrentPinCode','StudentAddress'),
        ('CurrentContactNo','StudentAddress'),
        ('PermanentLine1','StudentAddress'), ('PermanentLine2','StudentAddress'), ('PermanentPinCode','StudentAddress'),
        ('PermanentContactNo','StudentAddress'),
        ('StudentBankAccount','StudentAdditionalInformation'), ('IFSC','StudentAdditionalInformation');

    IF EXISTS (SELECT 1 FROM @Changes C WHERE NOT EXISTS (SELECT 1 FROM @Known K WHERE K.FieldKey = C.ColumnName))
    BEGIN SET @Result = -2; RETURN; END

    -- new values: trimmed, empty -> NULL (one row per key; last one wins if sent twice)
    DECLARE @New TABLE (FieldKey SYSNAME PRIMARY KEY, NewValue NVARCHAR(MAX) NULL);
    INSERT INTO @New (FieldKey, NewValue)
    SELECT ColumnName, MAX(NULLIF(LTRIM(RTRIM(NewValue)), ''))
    FROM @Changes
    GROUP BY ColumnName;

    IF EXISTS (SELECT 1 FROM @New WHERE FieldKey IN ('DateOfBirth','FatherDOB','MotherDOB')
                                    AND NewValue IS NOT NULL AND TRY_CONVERT(DATE, NewValue, 23) IS NULL)
       OR EXISTS (SELECT 1 FROM @New WHERE FieldKey IN ('FatherAnnualIncome','MotherAnnualIncome')
                                    AND NewValue IS NOT NULL AND TRY_CONVERT(NUMERIC(18,0), NewValue) IS NULL)
    BEGIN SET @Result = -4; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        ------------------------------------------------------------------
        -- 1. old values (same text form as the Get SP)
        ------------------------------------------------------------------
        DECLARE @Old TABLE (FieldKey SYSNAME PRIMARY KEY, OldValue NVARCHAR(MAX) NULL);

        INSERT INTO @Old (FieldKey, OldValue)
        SELECT F.FieldKey, F.OldValue
        FROM dbo.StudentMaster S WITH (UPDLOCK, HOLDLOCK)
        CROSS APPLY (VALUES
            ('DateOfBirth',        CONVERT(NVARCHAR(10), S.DateOfBirth, 23)),
            ('AadhaarNo',          CAST(S.AadhaarNo AS NVARCHAR(MAX))),
            ('MobileNo',           CAST(S.MobileNo AS NVARCHAR(MAX))),
            ('EmailId',            CAST(S.EmailId AS NVARCHAR(MAX))),
            ('BloodGroup',         CAST(S.BloodGroup AS NVARCHAR(MAX))),
            ('MedicalInformation', CAST(S.MedicalInformation AS NVARCHAR(MAX))),
            ('NatureOfDisability', CAST(S.NatureOfDisability AS NVARCHAR(MAX)))
        ) F (FieldKey, OldValue)
        WHERE S.StudentId = @StudentId AND S.GroupCode = @GroupCode AND S.BranchCode = @BranchCode;

        INSERT INTO @Old (FieldKey, OldValue)
        SELECT F.FieldKey, F.OldValue
        FROM (SELECT 1 AS One) X
        LEFT JOIN dbo.StudentParent P WITH (UPDLOCK, HOLDLOCK)
               ON P.StudentId = @StudentId AND P.GroupCode = @GroupCode AND P.BranchCode = @BranchCode
        CROSS APPLY (VALUES
            ('FatherContactNo',          CAST(P.FatherContactNo AS NVARCHAR(MAX))),
            ('FatherEMail',              CAST(P.FatherEMail AS NVARCHAR(MAX))),
            ('FatherAadhaarNo',          CAST(P.FatherAadhaarNo AS NVARCHAR(MAX))),
            ('FatherDOB',                CONVERT(NVARCHAR(10), P.FatherDOB, 23)),
            ('FatherAnnualIncome',       CAST(P.FatherAnnualIncome AS NVARCHAR(MAX))),
            ('FatherOfficeContactNo',    CAST(P.FatherOfficeContactNo AS NVARCHAR(MAX))),
            ('FatherOfficeAddress',      CAST(P.FatherOfficeAddress AS NVARCHAR(MAX))),
            ('MotherContactNo',          CAST(P.MotherContactNo AS NVARCHAR(MAX))),
            ('MotherEMail',              CAST(P.MotherEMail AS NVARCHAR(MAX))),
            ('MotherAadhaarNo',          CAST(P.MotherAadhaarNo AS NVARCHAR(MAX))),
            ('MotherDOB',                CONVERT(NVARCHAR(10), P.MotherDOB, 23)),
            ('MotherAnnualIncome',       CAST(P.MotherAnnualIncome AS NVARCHAR(MAX))),
            ('MotherOfficeContactNo',    CAST(P.MotherOfficeContactNo AS NVARCHAR(MAX))),
            ('MotherOfficeAddress',      CAST(P.MotherOfficeAddress AS NVARCHAR(MAX))),
            ('SMSMobileNo',              CAST(P.SMSMobileNo AS NVARCHAR(MAX))),
            ('GuardianContactNo',        CAST(P.GuardianContactNo AS NVARCHAR(MAX))),
            ('GuardianEmail',            CAST(P.GuardianEmail AS NVARCHAR(MAX))),
            ('GuardianAddress',          CAST(P.GuardianAddress AS NVARCHAR(MAX))),
            ('EmergencyPersonContactNo', CAST(P.EmergencyPersonContactNo AS NVARCHAR(MAX))),
            ('EmergencyPersonAddress',   CAST(P.EmergencyPersonAddress AS NVARCHAR(MAX)))
        ) F (FieldKey, OldValue);

        DECLARE @CurAddrId INT, @PerAddrId INT;
        SELECT TOP (1) @CurAddrId = AddressID FROM dbo.StudentAddress WITH (UPDLOCK, HOLDLOCK)
        WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND AddressType = 'C'
        ORDER BY AddressID DESC;
        SELECT TOP (1) @PerAddrId = AddressID FROM dbo.StudentAddress WITH (UPDLOCK, HOLDLOCK)
        WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND AddressType = 'P'
        ORDER BY AddressID DESC;

        INSERT INTO @Old (FieldKey, OldValue)
        SELECT F.FieldKey, F.OldValue
        FROM (SELECT 1 AS One) X
        LEFT JOIN dbo.StudentAddress C ON C.AddressID = @CurAddrId
        LEFT JOIN dbo.StudentAddress PA ON PA.AddressID = @PerAddrId
        CROSS APPLY (VALUES
            ('CurrentLine1',       CAST(C.Line1 AS NVARCHAR(MAX))),
            ('CurrentLine2',       CAST(C.Line2 AS NVARCHAR(MAX))),
            ('CurrentPinCode',     CAST(C.PinCode AS NVARCHAR(MAX))),
            ('CurrentContactNo',   CAST(C.ContactNo AS NVARCHAR(MAX))),
            ('PermanentLine1',     CAST(PA.Line1 AS NVARCHAR(MAX))),
            ('PermanentLine2',     CAST(PA.Line2 AS NVARCHAR(MAX))),
            ('PermanentPinCode',   CAST(PA.PinCode AS NVARCHAR(MAX))),
            ('PermanentContactNo', CAST(PA.ContactNo AS NVARCHAR(MAX)))
        ) F (FieldKey, OldValue);

        INSERT INTO @Old (FieldKey, OldValue)
        SELECT F.FieldKey, F.OldValue
        FROM (SELECT 1 AS One) X
        OUTER APPLY (SELECT TOP (1) AI.StudentBankAccount, AI.IFSC
                     FROM dbo.StudentAdditionalInformation AI WITH (UPDLOCK, HOLDLOCK)
                     WHERE AI.StudentId = @StudentId AND AI.GroupCode = @GroupCode AND AI.BranchCode = @BranchCode
                     ORDER BY CASE WHEN AI.SessionId = @SessionId THEN 0 ELSE 1 END, AI.SeedId DESC) B
        CROSS APPLY (VALUES
            ('StudentBankAccount', CAST(B.StudentBankAccount AS NVARCHAR(MAX))),
            ('IFSC',               CAST(B.IFSC AS NVARCHAR(MAX)))
        ) F (FieldKey, OldValue);

        ------------------------------------------------------------------
        -- 2. what really changed (normalised compare; dates / numbers by value)
        ------------------------------------------------------------------
        DECLARE @Diff TABLE (FieldKey SYSNAME PRIMARY KEY, TableName SYSNAME, OldValue NVARCHAR(MAX), NewValue NVARCHAR(MAX));

        INSERT INTO @Diff (FieldKey, TableName, OldValue, NewValue)
        SELECT N.FieldKey, K.TableName, NULLIF(LTRIM(RTRIM(O.OldValue)), ''),
               CASE WHEN N.FieldKey IN ('DateOfBirth','FatherDOB','MotherDOB')
                        THEN CONVERT(NVARCHAR(10), TRY_CONVERT(DATE, N.NewValue, 23), 23)
                    WHEN N.FieldKey IN ('FatherAnnualIncome','MotherAnnualIncome')
                        THEN CAST(TRY_CONVERT(NUMERIC(18,0), N.NewValue) AS NVARCHAR(MAX))
                    WHEN N.FieldKey = 'IFSC' THEN UPPER(N.NewValue)
                    ELSE N.NewValue END
        FROM @New N
        INNER JOIN @Known K ON K.FieldKey = N.FieldKey
        LEFT JOIN @Old O ON O.FieldKey = N.FieldKey;

        DELETE FROM @Diff WHERE ISNULL(OldValue, N'') = ISNULL(NewValue, N'');

        SELECT @ChangedCount = COUNT(1) FROM @Diff;
        IF @ChangedCount = 0
        BEGIN
            COMMIT TRANSACTION;
            SET @Result = 3;
            RETURN;
        END

        -- has / value helpers (only changed fields are touched)
        DECLARE @Has TABLE (FieldKey SYSNAME PRIMARY KEY);
        INSERT INTO @Has SELECT FieldKey FROM @Diff;

        DECLARE @V TABLE (FieldKey SYSNAME PRIMARY KEY, Val NVARCHAR(MAX));
        INSERT INTO @V SELECT FieldKey, NewValue FROM @Diff;

        ------------------------------------------------------------------
        -- 3a. StudentMaster
        ------------------------------------------------------------------
        IF EXISTS (SELECT 1 FROM @Diff WHERE TableName = 'StudentMaster')
        BEGIN
            UPDATE S SET
                DateOfBirth        = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'DateOfBirth')        THEN TRY_CONVERT(DATE, (SELECT Val FROM @V WHERE FieldKey = 'DateOfBirth'), 23) ELSE S.DateOfBirth END,
                AadhaarNo          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'AadhaarNo')          THEN (SELECT Val FROM @V WHERE FieldKey = 'AadhaarNo')          ELSE S.AadhaarNo END,
                MobileNo           = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MobileNo')           THEN (SELECT Val FROM @V WHERE FieldKey = 'MobileNo')           ELSE S.MobileNo END,
                EmailId            = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'EmailId')            THEN (SELECT Val FROM @V WHERE FieldKey = 'EmailId')            ELSE S.EmailId END,
                BloodGroup         = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'BloodGroup')         THEN (SELECT Val FROM @V WHERE FieldKey = 'BloodGroup')         ELSE S.BloodGroup END,
                MedicalInformation = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MedicalInformation') THEN (SELECT Val FROM @V WHERE FieldKey = 'MedicalInformation') ELSE S.MedicalInformation END,
                NatureOfDisability = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'NatureOfDisability') THEN (SELECT Val FROM @V WHERE FieldKey = 'NatureOfDisability') ELSE S.NatureOfDisability END,
                UpdateOn           = GETDATE()
            FROM dbo.StudentMaster S
            WHERE S.StudentId = @StudentId AND S.GroupCode = @GroupCode AND S.BranchCode = @BranchCode;

            -- same history row as Usp_StudentPersonalDetails
            INSERT INTO dbo.StudentMasterHistory(AdmissionNo, GroupCode, BranchCode, RegistrationId, StudentId, SessionId, FirstName, MiddleName,
                LastName, Gender, DateOfBirth, ClassCode, SectionId, BloodGroup, MedicalInformation, Nationality, SocialCategory, Religion, Background,
                MotherTongue, HouseNo, IsTransportRequired, IsHostelRequired, IsSMSRequired, IsEmailRequired, IsNRI, ControlNo, RouteDistance,
                SpecialComments, AdmissionDate, ImagePath, IsValid, CreatedBy, CreatedDate, AadhaarNo, GsrnNo, IsDisability, NatureOfDisability,
                LastSchool, StateCode, EWSId, Caste, RecommendationBy, EmployeeApproched, RecommendationDocPath, RecommendationDocFile, ApaarId, PENNo, FamilyId,
                EmailId, MobileNo, TransportAppliedFrom, CBSEGamesId, FeeTemplateId)
            SELECT AdmissionNo, GroupCode, BranchCode, RegistrationId, StudentId, SessionId, FirstName, MiddleName, LastName, Gender, DateOfBirth,
                ClassCode, SectionId, BloodGroup, MedicalInformation, Nationality, SocialCategory, Religion, Background, MotherTongue, HouseNo,
                IsTransportRequired, IsHostelRequired, IsSMSRequired, IsEmailRequired, IsNRI, ControlNo, RouteDistance, SpecialComments,
                AdmissionDate, ImagePath, IsValid, 'DPDP_' + LEFT(@By, 45), GETDATE(), AadhaarNo, GsrnNo, IsDisability, NatureOfDisability, LastSchool, StateCode, EWSId, Caste,
                RecommendationBy, EmployeeApproched, RecommendationDocPath, RecommendationDocFile, ApaarId, PENNo, FamilyId, EmailId, MobileNo, TransportAppliedFrom, CBSEGamesId, FeeTemplateId
            FROM dbo.StudentMaster
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId;
        END

        ------------------------------------------------------------------
        -- 3b. StudentParent
        ------------------------------------------------------------------
        IF EXISTS (SELECT 1 FROM @Diff WHERE TableName = 'StudentParent')
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.StudentParent WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
                INSERT INTO dbo.StudentParent (StudentId, GroupCode, BranchCode, CreatedDate, CreatedBy)
                VALUES (@StudentId, @GroupCode, @BranchCode, GETDATE(), @By);

            UPDATE P SET
                FatherContactNo          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherContactNo')          THEN (SELECT Val FROM @V WHERE FieldKey = 'FatherContactNo')          ELSE P.FatherContactNo END,
                FatherEMail              = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherEMail')              THEN (SELECT Val FROM @V WHERE FieldKey = 'FatherEMail')              ELSE P.FatherEMail END,
                FatherAadhaarNo          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherAadhaarNo')          THEN (SELECT Val FROM @V WHERE FieldKey = 'FatherAadhaarNo')          ELSE P.FatherAadhaarNo END,
                FatherDOB                = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherDOB')                THEN TRY_CONVERT(DATE, (SELECT Val FROM @V WHERE FieldKey = 'FatherDOB'), 23) ELSE P.FatherDOB END,
                FatherAnnualIncome       = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherAnnualIncome')       THEN TRY_CONVERT(NUMERIC(18,0), (SELECT Val FROM @V WHERE FieldKey = 'FatherAnnualIncome')) ELSE P.FatherAnnualIncome END,
                FatherOfficeContactNo    = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherOfficeContactNo')    THEN (SELECT Val FROM @V WHERE FieldKey = 'FatherOfficeContactNo')    ELSE P.FatherOfficeContactNo END,
                FatherOfficeAddress      = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'FatherOfficeAddress')      THEN (SELECT Val FROM @V WHERE FieldKey = 'FatherOfficeAddress')      ELSE P.FatherOfficeAddress END,
                MotherContactNo          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherContactNo')          THEN (SELECT Val FROM @V WHERE FieldKey = 'MotherContactNo')          ELSE P.MotherContactNo END,
                MotherEMail              = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherEMail')              THEN (SELECT Val FROM @V WHERE FieldKey = 'MotherEMail')              ELSE P.MotherEMail END,
                MotherAadhaarNo          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherAadhaarNo')          THEN (SELECT Val FROM @V WHERE FieldKey = 'MotherAadhaarNo')          ELSE P.MotherAadhaarNo END,
                MotherDOB                = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherDOB')                THEN TRY_CONVERT(DATE, (SELECT Val FROM @V WHERE FieldKey = 'MotherDOB'), 23) ELSE P.MotherDOB END,
                MotherAnnualIncome       = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherAnnualIncome')       THEN TRY_CONVERT(NUMERIC(18,0), (SELECT Val FROM @V WHERE FieldKey = 'MotherAnnualIncome')) ELSE P.MotherAnnualIncome END,
                MotherOfficeContactNo    = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherOfficeContactNo')    THEN (SELECT Val FROM @V WHERE FieldKey = 'MotherOfficeContactNo')    ELSE P.MotherOfficeContactNo END,
                MotherOfficeAddress      = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'MotherOfficeAddress')      THEN (SELECT Val FROM @V WHERE FieldKey = 'MotherOfficeAddress')      ELSE P.MotherOfficeAddress END,
                SMSMobileNo              = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'SMSMobileNo')              THEN (SELECT Val FROM @V WHERE FieldKey = 'SMSMobileNo')              ELSE P.SMSMobileNo END,
                GuardianContactNo        = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'GuardianContactNo')        THEN (SELECT Val FROM @V WHERE FieldKey = 'GuardianContactNo')        ELSE P.GuardianContactNo END,
                GuardianEmail            = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'GuardianEmail')            THEN (SELECT Val FROM @V WHERE FieldKey = 'GuardianEmail')            ELSE P.GuardianEmail END,
                GuardianAddress          = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'GuardianAddress')          THEN (SELECT Val FROM @V WHERE FieldKey = 'GuardianAddress')          ELSE P.GuardianAddress END,
                EmergencyPersonContactNo = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'EmergencyPersonContactNo') THEN (SELECT Val FROM @V WHERE FieldKey = 'EmergencyPersonContactNo') ELSE P.EmergencyPersonContactNo END,
                EmergencyPersonAddress   = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'EmergencyPersonAddress')   THEN (SELECT Val FROM @V WHERE FieldKey = 'EmergencyPersonAddress')   ELSE P.EmergencyPersonAddress END
            FROM dbo.StudentParent P
            WHERE P.StudentId = @StudentId AND P.GroupCode = @GroupCode AND P.BranchCode = @BranchCode;

            -- same history row as Usp_StudentParentDetails
            INSERT INTO dbo.StudentParentHistory(ParentID, StudentId, GroupCode, BranchCode, FatherTitle, FatherName, FatherQualification, FatherOccupation, FatherOccupationOther,
                FatherDesignation, FatherEMail, FatherAnnualIncome, FatherContactNo, FatherAchievement, FatherImagePath, MotherTitle,
                MotherName, MotherQualification, MotherOccupation, MotherOccupationOther, MotherDesignation, MotherEMail, MotherAnnualIncome,
                MotherContactNo, MotherAchievement, MotherImagePath, SMSMobileNo, EmergencyPersonName, EmergencyPersonRelationShip,
                EmergencyPersonContactNo, CreatedDate, CreatedBy, FatherDOB, FatherCollege,
                FatherOrganisation, FatherOfficeContactNo, FatherOfficeAddress, MotherDOB, MotherCollege,
                MotherOrganisation, MotherOfficeContactNo, MotherOfficeAddress, ContactEmail, EmergencyPersonAddress,
                GuardianName, GuardianEmail, GuardianContactNo, GuardianRelationship, GuardianAddress)
            SELECT ParentID, StudentId, GroupCode, BranchCode, FatherTitle, FatherName, FatherQualification, FatherOccupation, FatherOccupationOther,
                FatherDesignation, FatherEMail, FatherAnnualIncome, FatherContactNo, FatherAchievement, FatherImagePath,
                MotherTitle, MotherName, MotherQualification, MotherOccupation, MotherOccupationOther, MotherDesignation, MotherEMail, MotherAnnualIncome,
                MotherContactNo, MotherAchievement, MotherImagePath, SMSMobileNo, EmergencyPersonName, EmergencyPersonRelationShip,
                EmergencyPersonContactNo, GETDATE(), 'DPDP_' + LEFT(@By, 45), FatherDOB, FatherCollege, FatherOrganisation, FatherOfficeContactNo,
                FatherOfficeAddress, MotherDOB, MotherCollege, MotherOrganisation, MotherOfficeContactNo, MotherOfficeAddress, ContactEmail, EmergencyPersonAddress,
                GuardianName, GuardianEmail, GuardianContactNo, GuardianRelationship, GuardianAddress
            FROM dbo.StudentParent
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId;
        END

        ------------------------------------------------------------------
        -- 3c. StudentAddress (C = current, P = permanent)
        ------------------------------------------------------------------
        IF EXISTS (SELECT 1 FROM @Diff WHERE FieldKey LIKE 'Current%')
        BEGIN
            IF @CurAddrId IS NULL
            BEGIN
                INSERT INTO dbo.StudentAddress (GroupCode, BranchCode, StudentId, AddressTo, AddressType, CreatedDate, CreatedBy)
                VALUES (@GroupCode, @BranchCode, @StudentId, 'Student', 'C', GETDATE(), @By);
                SET @CurAddrId = SCOPE_IDENTITY();
            END

            UPDATE A SET
                Line1     = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'CurrentLine1')     THEN (SELECT Val FROM @V WHERE FieldKey = 'CurrentLine1')     ELSE A.Line1 END,
                Line2     = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'CurrentLine2')     THEN (SELECT Val FROM @V WHERE FieldKey = 'CurrentLine2')     ELSE A.Line2 END,
                PinCode   = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'CurrentPinCode')   THEN (SELECT Val FROM @V WHERE FieldKey = 'CurrentPinCode')   ELSE A.PinCode END,
                ContactNo = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'CurrentContactNo') THEN (SELECT Val FROM @V WHERE FieldKey = 'CurrentContactNo') ELSE A.ContactNo END
            FROM dbo.StudentAddress A
            WHERE A.AddressID = @CurAddrId;

            INSERT INTO dbo.StudentAddressHistory(AddressID, GroupCode, BranchCode, StudentId, Line1, Line2, DistrictId, StateId,
                CountryId, PinCode, ContactNo, AddressTo, AddressType, CreatedDate, CreatedBy)
            SELECT AddressID, GroupCode, BranchCode, StudentId, Line1, Line2, DistrictId, StateId, CountryId, PinCode, ContactNo, AddressTo,
                AddressType, GETDATE(), 'DPDP_' + LEFT(@By, 45)
            FROM dbo.StudentAddress WHERE AddressID = @CurAddrId;
        END

        IF EXISTS (SELECT 1 FROM @Diff WHERE FieldKey LIKE 'Permanent%')
        BEGIN
            IF @PerAddrId IS NULL
            BEGIN
                INSERT INTO dbo.StudentAddress (GroupCode, BranchCode, StudentId, AddressTo, AddressType, CreatedDate, CreatedBy)
                VALUES (@GroupCode, @BranchCode, @StudentId, 'Student', 'P', GETDATE(), @By);
                SET @PerAddrId = SCOPE_IDENTITY();
            END

            UPDATE A SET
                Line1     = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'PermanentLine1')     THEN (SELECT Val FROM @V WHERE FieldKey = 'PermanentLine1')     ELSE A.Line1 END,
                Line2     = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'PermanentLine2')     THEN (SELECT Val FROM @V WHERE FieldKey = 'PermanentLine2')     ELSE A.Line2 END,
                PinCode   = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'PermanentPinCode')   THEN (SELECT Val FROM @V WHERE FieldKey = 'PermanentPinCode')   ELSE A.PinCode END,
                ContactNo = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'PermanentContactNo') THEN (SELECT Val FROM @V WHERE FieldKey = 'PermanentContactNo') ELSE A.ContactNo END
            FROM dbo.StudentAddress A
            WHERE A.AddressID = @PerAddrId;

            INSERT INTO dbo.StudentAddressHistory(AddressID, GroupCode, BranchCode, StudentId, Line1, Line2, DistrictId, StateId,
                CountryId, PinCode, ContactNo, AddressTo, AddressType, CreatedDate, CreatedBy)
            SELECT AddressID, GroupCode, BranchCode, StudentId, Line1, Line2, DistrictId, StateId, CountryId, PinCode, ContactNo, AddressTo,
                AddressType, GETDATE(), 'DPDP_' + LEFT(@By, 45)
            FROM dbo.StudentAddress WHERE AddressID = @PerAddrId;
        END

        ------------------------------------------------------------------
        -- 3d. StudentAdditionalInformation (bank account belongs to the student: every session row)
        ------------------------------------------------------------------
        IF EXISTS (SELECT 1 FROM @Diff WHERE TableName = 'StudentAdditionalInformation')
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.StudentAdditionalInformation
                           WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
                INSERT INTO dbo.StudentAdditionalInformation (GroupCode, BranchCode, StudentId, SessionId, CreatedBy, CreatedDate)
                VALUES (@GroupCode, @BranchCode, @StudentId, @SessionId, @By, GETDATE());

            UPDATE AI SET
                StudentBankAccount = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'StudentBankAccount') THEN (SELECT Val FROM @V WHERE FieldKey = 'StudentBankAccount') ELSE AI.StudentBankAccount END,
                IFSC               = CASE WHEN EXISTS (SELECT 1 FROM @Has WHERE FieldKey = 'IFSC')               THEN (SELECT Val FROM @V WHERE FieldKey = 'IFSC')               ELSE AI.IFSC END
            FROM dbo.StudentAdditionalInformation AI
            WHERE AI.StudentId = @StudentId AND AI.GroupCode = @GroupCode AND AI.BranchCode = @BranchCode;
        END

        ------------------------------------------------------------------
        -- 4. transaction log: old value + new value per field, then the reason
        ------------------------------------------------------------------
        DECLARE @Audit dbo.ColumnChangeType;

        DECLARE @Tables TABLE (TableName SYSNAME PRIMARY KEY);
        INSERT INTO @Tables SELECT DISTINCT TableName FROM @Diff;

        DECLARE @T SYSNAME;
        WHILE EXISTS (SELECT 1 FROM @Tables)
        BEGIN
            SELECT TOP (1) @T = TableName FROM @Tables ORDER BY TableName;

            DELETE FROM @Audit;
            INSERT INTO @Audit (ColumnName, OldValue, NewValue)
            SELECT FieldKey, OldValue, NewValue FROM @Diff WHERE TableName = @T;

            EXEC dbo.Usp_SaveTransactionLog
                @ReferenceKey = @RefKey, @ReferenceId = @RefId,
                @TableName = @T, @OperationType = 'EDIT',
                @TransBy = @By, @IP = @LogIP, @Changes = @Audit;

            DELETE FROM @Tables WHERE TableName = @T;
        END

        DELETE FROM @Audit;
        INSERT INTO @Audit (ColumnName, OldValue, NewValue)
        VALUES ('UpdateReason', NULL, LEFT(LTRIM(RTRIM(@Reason)), 300));

        EXEC dbo.Usp_SaveTransactionLog
            @ReferenceKey = @RefKey, @ReferenceId = @RefId,
            @TableName = 'DPDP_StudentSensitive', @OperationType = 'EDIT',
            @TransBy = @By, @IP = @LogIP, @Changes = @Audit;

        COMMIT TRANSACTION;
        SET @Result = 1;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        INSERT INTO dbo.Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Admission', 'StudentSensitiveDetails', 'USP_DPDP_UpdateStudentSensitiveDetails',
                ERROR_MESSAGE() + ' Line:' + CAST(ERROR_LINE() AS VARCHAR(10)), GETDATE());

        SELECT @Result = 0, @ChangedCount = 0;
    END CATCH
END
GO
