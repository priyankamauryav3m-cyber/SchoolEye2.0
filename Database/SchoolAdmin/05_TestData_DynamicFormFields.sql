/* ============================================================================
   TEST DATA - Dynamic Form Controller fields (GroupCode 01 / BranchCode 01)
     Swimming  (SAId 1): 20 fields, prefix SW_
     Badmitan  (SAId 2): 30 fields, prefix BD_
   Every field type (Textbox, Dropdown, Radio, Checkbox, DateTime) and every
   data type (string, int, long, decimal, double, datetime, bit).
   Saved through USP_AddUpdateDynamicFormController2 (adds the OtherActivities column).
   Field names are unique per branch (the SP refuses a name already active in the
   branch), so each activity has its own prefix.

   Rollback (removes the rows AND the test columns of OtherActivities):
     DELETE FROM DynamicFormController WHERE GroupCode='01' AND BranchCode='01'
        AND (FieldName LIKE 'SW[_]%' OR FieldName LIKE 'BD[_]%');
     -- then for each column: ALTER TABLE dbo.OtherActivities DROP COLUMN [SW_...];
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @Fields TABLE (
    RowNo INT IDENTITY(1, 1), SAId INT, FieldName NVARCHAR(100), DisplayName NVARCHAR(100), DisplayOrder INT,
    MaxLength INT NULL, FieldType NVARCHAR(50), DataType NVARCHAR(50), OptionValues NVARCHAR(MAX) NULL, IsMandatory BIT);

DECLARE @Classes NVARCHAR(200) = N'Nursery,LKG,UKG,I,II,III,IV,V,VI,VII,VIII,IX,X,XI,XII';
DECLARE @Blood   NVARCHAR(100) = N'A+,A-,B+,B-,O+,O-,AB+,AB-';

-- ---------------- Swimming (SAId 1): 20 ----------------
INSERT INTO @Fields (SAId, FieldName, DisplayName, DisplayOrder, MaxLength, FieldType, DataType, OptionValues, IsMandatory) VALUES
(1, N'SW_StudentName',    N'Student Name',              1, 100, N'Textbox',  N'string',   NULL, 1),
(1, N'SW_FatherName',     N'Father Name',               2, 100, N'Textbox',  N'string',   NULL, 1),
(1, N'SW_MotherName',     N'Mother Name',               3, 100, N'Textbox',  N'string',   NULL, 0),
(1, N'SW_Gender',         N'Gender',                    4,  10, N'Radio',    N'string',   N'Male,Female,Other', 1),
(1, N'SW_DateOfBirth',    N'Date of Birth',             5, NULL, N'DateTime', N'datetime', NULL, 1),
(1, N'SW_Age',            N'Age (Years)',               6, NULL, N'Textbox',  N'int',      NULL, 1),
(1, N'SW_MobileNo',       N'Mobile Number',             7, NULL, N'Textbox',  N'long',     NULL, 1),
(1, N'SW_AltMobileNo',    N'Alternate Mobile Number',   8, NULL, N'Textbox',  N'long',     NULL, 0),
(1, N'SW_Email',          N'Email ID',                  9, 100, N'Textbox',  N'string',   NULL, 0),
(1, N'SW_Class',          N'Class',                    10,  20, N'Dropdown', N'string',   @Classes, 1),
(1, N'SW_Section',        N'Section',                  11,   5, N'Dropdown', N'string',   N'A,B,C,D,E', 0),
(1, N'SW_HeightCm',       N'Height (cm)',              12, NULL, N'Textbox',  N'decimal',  NULL, 0),
(1, N'SW_WeightKg',       N'Weight (kg)',              13, NULL, N'Textbox',  N'double',   NULL, 0),
(1, N'SW_SwimLevel',      N'Swimming Level',           14,  20, N'Radio',    N'string',   N'Beginner,Intermediate,Advanced', 1),
(1, N'SW_PreferredTime',  N'Preferred Time',           15,  20, N'Dropdown', N'string',   N'Morning,Afternoon,Evening', 0),
(1, N'SW_MedicalIssues',  N'Medical Issues',           16, 200, N'Checkbox', N'string',   N'Asthma,Allergy,Epilepsy,Ear Infection,None', 0),
(1, N'SW_CanSwim',        N'Already Knows Swimming',   17, NULL, N'Checkbox', N'bit',      NULL, 0),
(1, N'SW_BloodGroup',     N'Blood Group',              18,   5, N'Dropdown', N'string',   @Blood, 0),
(1, N'SW_Address',        N'Address',                  19, 250, N'Textbox',  N'string',   NULL, 1),
(1, N'SW_ParentConsent',  N'I agree to the terms and conditions', 20, NULL, N'Checkbox', N'bit', NULL, 1);

-- ---------------- Badmitan (SAId 2): 30 ----------------
INSERT INTO @Fields (SAId, FieldName, DisplayName, DisplayOrder, MaxLength, FieldType, DataType, OptionValues, IsMandatory) VALUES
(2, N'BD_StudentName',        N'Student First Name',        1, 100, N'Textbox',  N'string',   NULL, 1),
(2, N'BD_StudentLastName',    N'Student Last Name',         2, 100, N'Textbox',  N'string',   NULL, 0),
(2, N'BD_FatherName',         N'Father Name',               3, 100, N'Textbox',  N'string',   NULL, 1),
(2, N'BD_MotherName',         N'Mother Name',               4, 100, N'Textbox',  N'string',   NULL, 0),
(2, N'BD_Gender',             N'Gender',                    5,  10, N'Radio',    N'string',   N'Male,Female,Other', 1),
(2, N'BD_DateOfBirth',        N'Date of Birth',             6, NULL, N'DateTime', N'datetime', NULL, 1),
(2, N'BD_AdmissionNo',        N'Admission Number',          7,  20, N'Textbox',  N'string',   NULL, 1),
(2, N'BD_Class',              N'Class',                     8,  20, N'Dropdown', N'string',   @Classes, 1),
(2, N'BD_Section',            N'Section',                   9,   5, N'Dropdown', N'string',   N'A,B,C,D,E', 0),
(2, N'BD_RollNo',             N'Roll Number',              10, NULL, N'Textbox',  N'int',      NULL, 0),
(2, N'BD_MobileNo',           N'Mobile Number',            11, NULL, N'Textbox',  N'long',     NULL, 1),
(2, N'BD_WhatsAppNo',         N'WhatsApp Number',          12, NULL, N'Textbox',  N'long',     NULL, 0),
(2, N'BD_Email',              N'Email ID',                 13, 100, N'Textbox',  N'string',   NULL, 1),
(2, N'BD_HeightCm',           N'Height (cm)',              14, NULL, N'Textbox',  N'decimal',  NULL, 0),
(2, N'BD_WeightKg',           N'Weight (kg)',              15, NULL, N'Textbox',  N'double',   NULL, 0),
(2, N'BD_PlayingHand',        N'Playing Hand',             16,  10, N'Radio',    N'string',   N'Right,Left', 1),
(2, N'BD_ExperienceYears',    N'Experience (Years)',       17, NULL, N'Textbox',  N'int',      NULL, 0),
(2, N'BD_SkillLevel',         N'Skill Level',              18,  20, N'Dropdown', N'string',   N'Beginner,Intermediate,Advanced,Professional', 1),
(2, N'BD_OwnRacket',          N'Has Own Racket',           19, NULL, N'Checkbox', N'bit',      NULL, 0),
(2, N'BD_PreferredDays',      N'Preferred Days',           20, 100, N'Checkbox', N'string',   N'Monday,Tuesday,Wednesday,Thursday,Friday,Saturday', 0),
(2, N'BD_PreferredBatch',     N'Preferred Batch',          21,  20, N'Radio',    N'string',   N'Morning,Evening', 1),
(2, N'BD_TshirtSize',         N'T-Shirt Size',             22,   5, N'Dropdown', N'string',   N'XS,S,M,L,XL,XXL', 0),
(2, N'BD_FeeConcession',      N'Fee Concession Amount',    23, NULL, N'Textbox',  N'decimal',  NULL, 0),
(2, N'BD_PlayedTournament',   N'Played a Tournament Before', 24, 5, N'Radio',    N'string',   N'Yes,No', 0),
(2, N'BD_LastTournamentDate', N'Last Tournament Date',     25, NULL, N'DateTime', N'datetime', NULL, 0),
(2, N'BD_MedicalIssues',      N'Medical Issues',           26, 200, N'Checkbox', N'string',   N'Asthma,Allergy,Joint Pain,None', 0),
(2, N'BD_BloodGroup',         N'Blood Group',              27,   5, N'Dropdown', N'string',   @Blood, 0),
(2, N'BD_EmergencyContact',   N'Emergency Contact Number', 28, NULL, N'Textbox',  N'long',     NULL, 1),
(2, N'BD_Address',            N'Address',                  29, 250, N'Textbox',  N'string',   NULL, 1),
(2, N'BD_ParentConsent',      N'I agree to the terms and conditions', 30, NULL, N'Checkbox', N'bit', NULL, 1);

DECLARE @i INT = 1, @n INT = (SELECT COUNT(*) FROM @Fields), @Result INT;
DECLARE @SAId INT, @FieldName NVARCHAR(100), @DisplayName NVARCHAR(100), @DisplayOrder INT, @MaxLength INT,
        @FieldType NVARCHAR(50), @DataType NVARCHAR(50), @OptionValues NVARCHAR(MAX), @IsMandatory BIT;
DECLARE @Out TABLE (FieldName NVARCHAR(100), Result INT);

WHILE @i <= @n
BEGIN
    SELECT @SAId = SAId, @FieldName = FieldName, @DisplayName = DisplayName, @DisplayOrder = DisplayOrder, @MaxLength = MaxLength,
           @FieldType = FieldType, @DataType = DataType, @OptionValues = OptionValues, @IsMandatory = IsMandatory
    FROM @Fields WHERE RowNo = @i;

    SET @Result = NULL;
    EXEC dbo.USP_AddUpdateDynamicFormController2
        @GroupCode = '01', @BranchCode = '01', @DFCId = 0, @FieldName = @FieldName, @DisplayName = @DisplayName,
        @DisplayOrder = @DisplayOrder, @MaxLength = @MaxLength, @FieldType = @FieldType, @DataType = @DataType,
        @OptionValues = @OptionValues, @IsMandatory = @IsMandatory, @CreatedBy = 'TestData', @SAId = @SAId,
        @Result = @Result OUTPUT;

    INSERT INTO @Out VALUES (@FieldName, @Result);
    SET @i += 1;
END

-- 1 = added, -4 = already exists (re-run), anything else = check
SELECT Result, COUNT(*) AS Fields FROM @Out GROUP BY Result;
SELECT FieldName, Result FROM @Out WHERE Result NOT IN (1, -4);
