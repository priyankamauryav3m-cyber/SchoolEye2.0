/* Rollback of ADM_AddUpdateRegNoAddressInfo_Emergency.sql: puts back the procedure without the emergency contact.
   Emergency values already written to ADM_Registration stay as they are.
   Deploy the old web API together with this rollback: the new API sends the three emergency parameters,
   which the old procedure does not accept. */

CREATE OR ALTER PROC [dbo].[ADM_AddUpdateRegNoAddressInfo] @GroupCode VARCHAR(10),@BranchCode VARCHAR(10),@SessionId bigint,@RegistrationId bigint
,
 @line1 VARCHAR(150), @line2 VARCHAR(150), @Pincode VARCHAR(10), @ContactNo VARCHAR(50),@AddressTo VARCHAR(10), @AddressType CHAR(1), @CreatedBy VARCHAR(30)
AS
BEGIN
 IF EXISTS(SELECT * FROM ADM_AddressInformation WHERE
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
END
GO
