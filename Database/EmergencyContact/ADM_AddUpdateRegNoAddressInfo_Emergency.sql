/* Address tab (StudentDetails): the emergency contact person is saved with the current address,
   in the same ADM_Registration columns that ADM_UpdateChildDetails updates.
   The three new parameters default to NULL, so a caller that does not send them works as before.
   Rollback: ADM_AddUpdateRegNoAddressInfo_Rollback.sql */

CREATE OR ALTER PROC [dbo].[ADM_AddUpdateRegNoAddressInfo] @GroupCode VARCHAR(10),@BranchCode VARCHAR(10),@SessionId bigint,@RegistrationId bigint,
 @line1 VARCHAR(150), @line2 VARCHAR(150), @Pincode VARCHAR(10), @ContactNo VARCHAR(50),@AddressTo VARCHAR(10), @AddressType CHAR(1), @CreatedBy VARCHAR(30),
 @EmergencyPersonName VARCHAR(50) = NULL, @EmergencyPersonRelationShip VARCHAR(50) = NULL, @EmergencyPersonContactNo VARCHAR(50) = NULL
AS
BEGIN
 SET XACT_ABORT ON;
 BEGIN TRY
 BEGIN TRAN;

 IF EXISTS(SELECT 1 FROM ADM_AddressInformation WHERE
 SessionId = @SessionId AND RegistrationId = @RegistrationId AND AddressType= @AddressType and AddressTo = @AddressTo)
 BEGIN
	UPDATE ADM_AddressInformation SET Line1 = @line1, Line2 = @line2, PinCode = @Pincode, ContactNo = @ContactNo,
	AddressTo = @AddressTo, CreatedDate = GETDATE(), CreatedBy = @CreatedBy
	WHERE  SessionId = @SessionId AND
	RegistrationId = @RegistrationId AND AddressType= @AddressType  and AddressTo = @AddressTo
 END
 ELSE
 BEGIN
	INSERT INTO ADM_AddressInformation(SessionId, RegistrationId, Line1, Line2,
	PinCode, ContactNo, AddressTo, AddressType, CreatedDate, CreatedBy)
	VALUES( @SessionId, @RegistrationId, @Line1, @Line2,
	@PinCode, @ContactNo, @AddressTo, @AddressType, GETDATE(), @CreatedBy)
 END

 -- emergency contact is sent only with the current address
 IF @AddressType = 'C'
    AND (@EmergencyPersonName IS NOT NULL OR @EmergencyPersonRelationShip IS NOT NULL OR @EmergencyPersonContactNo IS NOT NULL)
 BEGIN
	UPDATE ADM_Registration SET EmergencyPersonName = @EmergencyPersonName,
	EmergencyPersonRelationShip = @EmergencyPersonRelationShip,
	EmergencyPersonContactNo = @EmergencyPersonContactNo
	WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND RegistrationId = @RegistrationId
 END

 COMMIT TRAN;
 END TRY
 BEGIN CATCH
	IF @@TRANCOUNT > 0 ROLLBACK TRAN;
	THROW;
 END CATCH
END
GO
