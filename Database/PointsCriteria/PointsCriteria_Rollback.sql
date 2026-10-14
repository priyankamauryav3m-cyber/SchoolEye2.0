/* Rollback of PointsCriteria_Save.sql: puts back the old two procedures.
   The new columns SingleParentDetail / IsDayCareRequired are left in place (NULL-able, unused by the old procedures);
   drop them only with approval — that removes the saved values. */

CREATE OR ALTER PROC [dbo].[ADM_UpdateOtherInformation] @GroupCode VARCHAR(10),@BranchCode VARCHAR(10),@SessionId bigint,@RegistrationId bigint,
@DistanceFromSchool VARCHAR(50),@CoreCategory INT,@IsReservedSeat INT,@FatherTransferableJob INT,@MoterTransferableJob INT,
@GirlChildOnly INT,@DefencePerson INT,@DefenceDetail VARCHAR(100),@Sibling INT,@SingleParent INT,@SingleFatherName VARCHAR(50),
@SingleMotherName VARCHAR(50),@SingleParentComment VARCHAR(100),@IsLegalDocumentHave INT,@CreatedBy VARCHAR(50),@isFatherAlumni int,
@FatherPassingYear varchar(10),@isMotherAlumni int,@MotherPassingYear varchar(10),@IsFirstBornChild int,
@IsStaffWard int, @IsChildBelowAge int, @IsTwinChild int ,@IsAdoptedChild int,
@fatherAlumniBranch varchar(50),@motherAlumniBranch varchar(50), @childCustody varchar(20), @hearingFrom varchar(200),
@FatherAlumniClass varchar(20),@MotherAlumniClass varchar(20)
AS
BEGIN
 IF EXISTS(SELECT * FROM ADM_OtherInformation WHERE SessionId=@SessionId AND RegistrationId= @RegistrationId)
 BEGIN
 UPDATE ADM_OtherInformation SET DistanceFromSchool = @DistanceFromSchool, CoreCategory = @CoreCategory, IsReservedSeat = @IsReservedSeat,
 FatherTransferableJob = @FatherTransferableJob, MoterTransferableJob = @MoterTransferableJob, GirlChildOnly = @GirlChildOnly,
 DefencePerson = @DefencePerson, DefenceDetail = @DefenceDetail, Sibling = @Sibling, SingleParent = @SingleParent,
 SingleFatherName = @SingleFatherName, SingleMotherName = @SingleMotherName, SingleParentComment = @SingleParentComment,
 IsLegalDocumentHave = @IsLegalDocumentHave, CreatedDate = GETDATE(), CreatedBy = @CreatedBy,IsFatherAlumni=@isFatherAlumni,
 FPassingYear=@FatherPassingYear,IsMotherAlumni=@isMotherAlumni,MPassingYear=@MotherPassingYear, IsFirstBornChild = @IsFirstBornChild,
 staffWard=@IsStaffWard,childBelowAge=@IsChildBelowAge,twinsChild=@IsTwinChild,IsadoptedChild=@IsAdoptedChild
 , FatherBranch = @fatherAlumniBranch, MotherBranch = @motherAlumniBranch, ChildCustody = @childCustody , HearingFrom = @HearingFrom
 , FatherAlumniClass =  @FatherAlumniClass, MotherAlumniClass = @MotherAlumniClass
 WHERE    SessionId=@SessionId AND RegistrationId= @RegistrationId
 END
 ELSE
 BEGIN
 INSERT INTO ADM_OtherInformation( SessionId, RegistrationId, DistanceFromSchool, CoreCategory,
 IsReservedSeat, FatherTransferableJob, MoterTransferableJob, GirlChildOnly, DefencePerson, DefenceDetail, Sibling,
 SingleParent, SingleFatherName, SingleMotherName, SingleParentComment, IsLegalDocumentHave, CreatedDate, CreatedBy,
 IsFatherAlumni,FPassingYear,IsMotherAlumni,MPassingYear, IsFirstBornChild,staffWard,childBelowAge,twinsChild,IsadoptedChild
 ,FatherBranch ,MotherBranch ,ChildCustody, HearingFrom, FatherAlumniClass, MotherAlumniClass)
 VALUES( @SessionId, @RegistrationId, @DistanceFromSchool, @CoreCategory,
 @IsReservedSeat, @FatherTransferableJob, @MoterTransferableJob, @GirlChildOnly, @DefencePerson, @DefenceDetail, @Sibling,
 @SingleParent, @SingleFatherName, @SingleMotherName, @SingleParentComment, @IsLegalDocumentHave, GETDATE(), @CreatedBy,
 @isFatherAlumni,@FatherPassingYear,@isMotherAlumni,@MotherPassingYear, @IsFirstBornChild,@IsStaffWard,@IsChildBelowAge,@IsTwinChild,@IsAdoptedChild
 ,@fatherAlumniBranch, @motherAlumniBranch, @childCustody ,@HearingFrom, @FatherAlumniClass, @MotherAlumniClass)
 END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ADM_GetChildOtherInformation]
(
    @SessionId bigint,@RegistrationId bigint
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM ADM_OtherInformation with(nolock)
    WHERE  SessionId = @SessionId AND RegistrationId = @RegistrationId;
END
GO
