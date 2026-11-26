/* =====================================================================================
   DPDP - Student Sensitive Details
   02. V3M_DPDP_GetStudentSensitiveDetails

   Full (unmasked) sensitive details of ONE student and the student's parents / guardian.
   Used only by the Student Sensitive Details page, after V3M_DPDP_CheckPiiAccess.

   Result set 1 : header (who the student is - not sensitive)
   Result set 2 : one row per sensitive field
                  FieldKey      -> same key the API / page / update SP use
                  FieldValue    -> raw value as text (dates yyyy-MM-dd)
                  ChangeCount   -> how many times the field was changed on this page
                  LastChangedOn / LastChangedBy

   Field keys (table.column):
     StudentMaster              : DateOfBirth, AadhaarNo, MobileNo, EmailId, BloodGroup,
                                  MedicalInformation, NatureOfDisability
     StudentParent              : Father.. / Mother.. (ContactNo, EMail, AadhaarNo, DOB, AnnualIncome,
                                  OfficeContactNo, OfficeAddress), SMSMobileNo,
                                  GuardianContactNo, GuardianEmail, GuardianAddress,
                                  EmergencyPersonContactNo, EmergencyPersonAddress
     StudentAddress (C / P)     : CurrentLine1, CurrentLine2, CurrentPinCode, CurrentContactNo,
                                  PermanentLine1, PermanentLine2, PermanentPinCode, PermanentContactNo
     StudentAdditionalInformation: StudentBankAccount, IFSC
   ===================================================================================== */

-- history / change-count lookups read Det_TransactionLog by reference (any module benefits)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Det_TransactionLog_Reference'
                                          AND object_id = OBJECT_ID('dbo.Det_TransactionLog'))
    CREATE NONCLUSTERED INDEX IX_Det_TransactionLog_Reference
        ON dbo.Det_TransactionLog (ReferenceKey, ReferenceId)
        INCLUDE (ColumnName, TransBy, TransDatetime);
GO

CREATE OR ALTER PROCEDURE dbo.V3M_DPDP_GetStudentSensitiveDetails
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @SessionId  BIGINT,
    @StudentId  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.StudentMaster WITH (NOLOCK)
                   WHERE StudentId = @StudentId AND GroupCode = @GroupCode AND BranchCode = @BranchCode)
    BEGIN
        -- empty header -> API answers "Student record was not found."
        SELECT CAST(NULL AS BIGINT) AS StudentId WHERE 1 = 0;
        RETURN;
    END

    ------------------------------------------------------------------
    -- 1. header
    ------------------------------------------------------------------
    SELECT TOP (1)
           S.StudentId,
           ISNULL(V.ControlNo COLLATE DATABASE_DEFAULT, S.ControlNo)                                       AS ControlNo,
           ISNULL(V.StudentName COLLATE DATABASE_DEFAULT, LTRIM(RTRIM(CONCAT(S.FirstName, ' ', S.MiddleName, ' ', S.LastName)))) AS StudentName,
           V.ClassSection,
           S.Gender,
           P.FatherName,
           P.MotherName,
           P.GuardianName,
           P.GuardianRelationship,
           P.EmergencyPersonName,
           P.EmergencyPersonRelationShip AS EmergencyPersonRelationship
    FROM dbo.StudentMaster S WITH (NOLOCK)
    LEFT JOIN dbo.vwStudentMiniDetails V WITH (NOLOCK)
           ON V.Studentid = S.StudentId AND V.SessionId = @SessionId
          AND V.GroupCode = S.GroupCode AND V.BranchCode = S.BranchCode
    LEFT JOIN dbo.StudentParent P WITH (NOLOCK)
           ON P.StudentId = S.StudentId AND P.GroupCode = S.GroupCode AND P.BranchCode = S.BranchCode
    WHERE S.StudentId = @StudentId AND S.GroupCode = @GroupCode AND S.BranchCode = @BranchCode;

    ------------------------------------------------------------------
    -- 2. fields (unpivoted) + change count
    ------------------------------------------------------------------
    DECLARE @RefId VARCHAR(50) = CAST(@StudentId AS VARCHAR(50));

    ;WITH Fields AS
    (
        SELECT F.FieldKey, F.FieldValue COLLATE DATABASE_DEFAULT AS FieldValue
        FROM dbo.StudentMaster S WITH (NOLOCK)
        CROSS APPLY (VALUES
            ('DateOfBirth',        CONVERT(NVARCHAR(10), S.DateOfBirth, 23)),
            ('AadhaarNo',          CAST(S.AadhaarNo AS NVARCHAR(MAX))),
            ('MobileNo',           CAST(S.MobileNo AS NVARCHAR(MAX))),
            ('EmailId',            CAST(S.EmailId AS NVARCHAR(MAX))),
            ('BloodGroup',         CAST(S.BloodGroup AS NVARCHAR(MAX))),
            ('MedicalInformation', CAST(S.MedicalInformation AS NVARCHAR(MAX))),
            ('NatureOfDisability', CAST(S.NatureOfDisability AS NVARCHAR(MAX)))
        ) F (FieldKey, FieldValue)
        WHERE S.StudentId = @StudentId AND S.GroupCode = @GroupCode AND S.BranchCode = @BranchCode

        UNION ALL
        SELECT F.FieldKey, F.FieldValue COLLATE DATABASE_DEFAULT AS FieldValue
        FROM (SELECT 1 AS One) X
        LEFT JOIN dbo.StudentParent P WITH (NOLOCK)
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
        ) F (FieldKey, FieldValue)

        UNION ALL
        SELECT F.FieldKey, F.FieldValue COLLATE DATABASE_DEFAULT AS FieldValue
        FROM (SELECT 1 AS One) X
        OUTER APPLY (SELECT TOP (1) A.Line1, A.Line2, A.PinCode, A.ContactNo
                     FROM dbo.StudentAddress A WITH (NOLOCK)
                     WHERE A.StudentId = @StudentId AND A.GroupCode = @GroupCode AND A.BranchCode = @BranchCode
                       AND A.AddressType = 'C'
                     ORDER BY A.AddressID DESC) C
        OUTER APPLY (SELECT TOP (1) A.Line1, A.Line2, A.PinCode, A.ContactNo
                     FROM dbo.StudentAddress A WITH (NOLOCK)
                     WHERE A.StudentId = @StudentId AND A.GroupCode = @GroupCode AND A.BranchCode = @BranchCode
                       AND A.AddressType = 'P'
                     ORDER BY A.AddressID DESC) PA
        CROSS APPLY (VALUES
            ('CurrentLine1',       CAST(C.Line1 AS NVARCHAR(MAX))),
            ('CurrentLine2',       CAST(C.Line2 AS NVARCHAR(MAX))),
            ('CurrentPinCode',     CAST(C.PinCode AS NVARCHAR(MAX))),
            ('CurrentContactNo',   CAST(C.ContactNo AS NVARCHAR(MAX))),
            ('PermanentLine1',     CAST(PA.Line1 AS NVARCHAR(MAX))),
            ('PermanentLine2',     CAST(PA.Line2 AS NVARCHAR(MAX))),
            ('PermanentPinCode',   CAST(PA.PinCode AS NVARCHAR(MAX))),
            ('PermanentContactNo', CAST(PA.ContactNo AS NVARCHAR(MAX)))
        ) F (FieldKey, FieldValue)

        UNION ALL
        SELECT F.FieldKey, F.FieldValue COLLATE DATABASE_DEFAULT AS FieldValue
        FROM (SELECT 1 AS One) X
        OUTER APPLY (SELECT TOP (1) AI.StudentBankAccount, AI.IFSC
                     FROM dbo.StudentAdditionalInformation AI WITH (NOLOCK)
                     WHERE AI.StudentId = @StudentId AND AI.GroupCode = @GroupCode AND AI.BranchCode = @BranchCode
                     ORDER BY CASE WHEN AI.SessionId = @SessionId THEN 0 ELSE 1 END, AI.SeedId DESC) B
        CROSS APPLY (VALUES
            ('StudentBankAccount', CAST(B.StudentBankAccount AS NVARCHAR(MAX))),
            ('IFSC',               CAST(B.IFSC AS NVARCHAR(MAX)))
        ) F (FieldKey, FieldValue)
    ),
    Changes AS
    (
        SELECT L.ColumnName,
               COUNT(1)             AS ChangeCount,
               MAX(L.TransDatetime) AS LastChangedOn
        FROM dbo.Det_TransactionLog L WITH (NOLOCK)
        WHERE L.ReferenceKey = 'DPDP_StudentSensitive'
          AND L.ReferenceId  = @RefId
          AND L.ColumnName  <> 'UpdateReason'
        GROUP BY L.ColumnName
    )
    SELECT Fd.FieldKey,
           Fd.FieldValue,
           ISNULL(Ch.ChangeCount, 0) AS ChangeCount,
           Ch.LastChangedOn,
           LastBy.TransBy            AS LastChangedBy
    FROM Fields Fd
    LEFT JOIN Changes Ch ON Ch.ColumnName = Fd.FieldKey
    OUTER APPLY (SELECT TOP (1) L.TransBy
                 FROM dbo.Det_TransactionLog L WITH (NOLOCK)
                 WHERE Ch.ColumnName IS NOT NULL
                   AND L.ReferenceKey = 'DPDP_StudentSensitive'
                   AND L.ReferenceId  = @RefId
                   AND L.ColumnName   = Fd.FieldKey
                 ORDER BY L.LogId DESC) LastBy;
END
GO
