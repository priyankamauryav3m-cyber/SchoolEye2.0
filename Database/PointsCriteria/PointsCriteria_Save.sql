/* =====================================================================
   Points Criteria (Registration Details > Points Criteria tab) — save & load
   ---------------------------------------------------------------------
   1. ADM_OtherInformation: 2 new NULL columns (no data changed / removed)
        SingleParentDetail  varchar(20)  Widow / Widower / Guardian / Other
        IsDayCareRequired   bit          availing day care facility after school hours
   2. ADM_UpdateOtherInformation: saves every Points Criteria field
        (TransferCity was never saved, passing years were cut to 10 chars,
         single parent detail / day care had no column),
        one transaction, returns Result: 1 saved, -1 invalid registration, 0 error
   3. ADM_GetChildOtherInformation: returns the columns with the names the
        PointsCriteria model uses (MoterTransferableJob, FPassingYear, StaffWard ...
        were never loaded back to the screen)
   Callers checked: only Infrastructure/Admin/RegistrationService.cs uses both procedures.
   Rollback: PointsCriteria_Rollback.sql (old procedures; new columns stay, they are NULL)
   ===================================================================== */

IF COL_LENGTH('dbo.ADM_OtherInformation', 'SingleParentDetail') IS NULL
    ALTER TABLE dbo.ADM_OtherInformation ADD SingleParentDetail VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.ADM_OtherInformation', 'IsDayCareRequired') IS NULL
    ALTER TABLE dbo.ADM_OtherInformation ADD IsDayCareRequired BIT NULL;
GO

CREATE OR ALTER PROCEDURE [dbo].[ADM_UpdateOtherInformation]
    @GroupCode VARCHAR(20), @BranchCode VARCHAR(20), @SessionId BIGINT, @RegistrationId BIGINT,
    @DistanceFromSchool VARCHAR(50), @CoreCategory INT, @IsReservedSeat BIT,
    @FatherTransferableJob BIT, @MoterTransferableJob BIT, @TransferCity VARCHAR(50) = NULL,
    @GirlChildOnly BIT, @DefencePerson BIT, @DefenceDetail VARCHAR(100), @Sibling BIT,
    @SingleParent INT, @SingleFatherName VARCHAR(50), @SingleMotherName VARCHAR(50), @SingleParentComment VARCHAR(100),
    @IsLegalDocumentHave BIT, @CreatedBy VARCHAR(50),
    @isFatherAlumni BIT, @FatherPassingYear VARCHAR(50), @isMotherAlumni BIT, @MotherPassingYear VARCHAR(50),
    @IsFirstBornChild BIT, @IsStaffWard BIT, @IsChildBelowAge BIT, @IsTwinChild BIT, @IsAdoptedChild BIT,
    @fatherAlumniBranch VARCHAR(100), @motherAlumniBranch VARCHAR(50), @childCustody VARCHAR(30), @hearingFrom VARCHAR(100),
    @FatherAlumniClass VARCHAR(20), @MotherAlumniClass VARCHAR(20),
    @SingleParentDetail VARCHAR(20) = NULL, @IsDayCareRequired BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@RegistrationId, 0) <= 0 OR ISNULL(@SessionId, 0) <= 0
    BEGIN
        SELECT -1 AS Result;   -- invalid registration / session
        RETURN;
    END

    -- custody and single parent detail only for a single parent; detail only with a custody
    IF ISNULL(@SingleParent, 0) = 0
        SELECT @childCustody = NULL, @SingleParentDetail = NULL;
    IF NULLIF(NULLIF(LTRIM(RTRIM(@childCustody)), ''), '0') IS NULL
        SELECT @childCustody = NULL, @SingleParentDetail = NULL;
    IF @SingleParentDetail NOT IN ('Widow', 'Widower', 'Guardian', 'Other')
        SET @SingleParentDetail = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.ADM_OtherInformation WITH (UPDLOCK, HOLDLOCK)
                   WHERE SessionId = @SessionId AND RegistrationId = @RegistrationId)
        BEGIN
            UPDATE dbo.ADM_OtherInformation SET
                DistanceFromSchool = @DistanceFromSchool, CoreCategory = @CoreCategory, IsReservedSeat = @IsReservedSeat,
                FatherTransferableJob = @FatherTransferableJob, MoterTransferableJob = @MoterTransferableJob, TransferCity = @TransferCity,
                GirlChildOnly = @GirlChildOnly, DefencePerson = @DefencePerson, DefenceDetail = @DefenceDetail, Sibling = @Sibling,
                SingleParent = @SingleParent, SingleFatherName = @SingleFatherName, SingleMotherName = @SingleMotherName,
                SingleParentComment = @SingleParentComment, IsLegalDocumentHave = @IsLegalDocumentHave,
                IsFatherAlumni = @isFatherAlumni, FPassingYear = @FatherPassingYear, IsMotherAlumni = @isMotherAlumni, MPassingYear = @MotherPassingYear,
                IsFirstBornChild = @IsFirstBornChild, StaffWard = @IsStaffWard, ChildBelowAge = @IsChildBelowAge,
                TwinsChild = @IsTwinChild, IsAdoptedChild = @IsAdoptedChild,
                FatherBranch = @fatherAlumniBranch, MotherBranch = @motherAlumniBranch, ChildCustody = @childCustody, HearingFrom = @hearingFrom,
                FatherAlumniClass = @FatherAlumniClass, MotherAlumniClass = @MotherAlumniClass,
                SingleParentDetail = @SingleParentDetail, IsDayCareRequired = @IsDayCareRequired,
                CreatedDate = GETDATE(), CreatedBy = @CreatedBy
            WHERE SessionId = @SessionId AND RegistrationId = @RegistrationId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.ADM_OtherInformation (
                SessionId, RegistrationId, DistanceFromSchool, CoreCategory, IsReservedSeat,
                FatherTransferableJob, MoterTransferableJob, TransferCity, GirlChildOnly, DefencePerson, DefenceDetail, Sibling,
                SingleParent, SingleFatherName, SingleMotherName, SingleParentComment, IsLegalDocumentHave,
                IsFatherAlumni, FPassingYear, IsMotherAlumni, MPassingYear,
                IsFirstBornChild, StaffWard, ChildBelowAge, TwinsChild, IsAdoptedChild,
                FatherBranch, MotherBranch, ChildCustody, HearingFrom, FatherAlumniClass, MotherAlumniClass,
                SingleParentDetail, IsDayCareRequired, CreatedDate, CreatedBy)
            VALUES (
                @SessionId, @RegistrationId, @DistanceFromSchool, @CoreCategory, @IsReservedSeat,
                @FatherTransferableJob, @MoterTransferableJob, @TransferCity, @GirlChildOnly, @DefencePerson, @DefenceDetail, @Sibling,
                @SingleParent, @SingleFatherName, @SingleMotherName, @SingleParentComment, @IsLegalDocumentHave,
                @isFatherAlumni, @FatherPassingYear, @isMotherAlumni, @MotherPassingYear,
                @IsFirstBornChild, @IsStaffWard, @IsChildBelowAge, @IsTwinChild, @IsAdoptedChild,
                @fatherAlumniBranch, @motherAlumniBranch, @childCustody, @hearingFrom, @FatherAlumniClass, @MotherAlumniClass,
                @SingleParentDetail, @IsDayCareRequired, GETDATE(), @CreatedBy);
        END

        COMMIT TRANSACTION;
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Procedure', 'ADM_UpdateOtherInformation', 'Save Points Criteria', ERROR_MESSAGE(), GETDATE());
        SELECT 0 AS Result;
    END CATCH
END
GO

-- column names = PointsCriteria model property names, so every field loads back on the screen
CREATE OR ALTER PROCEDURE [dbo].[ADM_GetChildOtherInformation]
(
    @SessionId BIGINT, @RegistrationId BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SessionId, RegistrationId,
        DistanceFromSchool, HearingFrom,
        ISNULL(CoreCategory, 1)                    AS CoreCategory,
        ISNULL(IsReservedSeat, 0)                  AS IsReservedSeat,
        ISNULL(GirlChildOnly, 0)                   AS IsGirlChildOnly,
        ISNULL(IsFirstBornChild, 0)                AS IsFirstBornChild,
        ISNULL(FatherTransferableJob, 0)           AS FatherTransferableJob,
        ISNULL(MoterTransferableJob, 0)            AS MotherTransferableJob,
        TransferCity,
        ISNULL(DefencePerson, 0)                   AS DefencePerson,
        DefenceDetail,
        ISNULL(IsFatherAlumni, 0)                  AS IsFatherAlumni,
        FatherAlumniClass,
        FPassingYear                               AS FatherPassingYear,
        FatherBranch                               AS FatherAlumniBranch,
        ISNULL(IsMotherAlumni, 0)                  AS IsMotherAlumni,
        MotherAlumniClass,
        MPassingYear                               AS MotherPassingYear,
        MotherBranch                               AS MotherAlumniBranch,
        CAST(CASE WHEN ISNULL(SingleParent, 0) = 0 THEN 0 ELSE 1 END AS BIT) AS SingleParent,
        ChildCustody,
        SingleParentDetail,
        SingleFatherName, SingleMotherName, SingleParentComment,
        ISNULL(IsLegalDocumentHave, 0)             AS IsLegalDocumentHave,
        ISNULL(StaffWard, 0)                       AS IsStaffWard,
        ISNULL(ChildBelowAge, 0)                   AS IsChildBelowAge,
        ISNULL(TwinsChild, 0)                      AS IsTwinChild,
        ISNULL(IsAdoptedChild, 0)                  AS IsAdoptedChild,
        ISNULL(IsDayCareRequired, 0)               AS IsDayCareRequired,
        ISNULL(Sibling, 0)                         AS Sibling
    FROM dbo.ADM_OtherInformation WITH (NOLOCK)
    WHERE SessionId = @SessionId AND RegistrationId = @RegistrationId;
END
GO
