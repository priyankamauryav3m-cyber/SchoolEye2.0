/*
    Rollback for 01_FIN_Usp_AddFeeHeadToStudentChallan_MultiMonth.sql
    Puts back the single-month version (@MonthNo int) exactly as it was deployed on 27-10-2026.
    Note: with this version Edit Invoice -> Add Fee Head fails again (the API sends dbo.TVP_MonthNo).
*/
CREATE OR ALTER PROCEDURE [dbo].[FIN_Usp_AddFeeHeadToStudentChallan] @StudentId bigint,@InvoiceId bigint,@HeadId int,@MonthNo int,@HeadAmount decimal(18,2),
@CreatedBy varchar(50),@Narration1 varchar(200),@Result int output    
AS    
DECLARE @ErrorCode int=0    
DECLARE @ChallanNo varchar(20)=''     -- InvoiceNo is varchar (e.g. 0101IN1481)
--DECLARE @HeadAmount decimal(18,2)=0    
DECLARE @Concession decimal(18,2)=0    
DECLARE @ReceivedAmt decimal(18,2)=0    
DECLARE @ReceivedDate datetime,@ChllanGeneratedDate datetime     
DECLARE @GroupCode varchar(50)='',@BranchCode varchar(50)='',@SessionId varchar(50),@Narration varchar(150)    
DECLARE @DiffereceAmount decimal(18,2)=0    
DECLARE @StudentBalance decimal(18,2)=0    
DECLARE @FeeHeadName varchar(50)=''    
DECLARE @DisplayOrder int=0    
DECLARE @tmpConcessionId int=0    
DECLARE @ClassCode varchar(50)=''    
    
Begin    
 ------------------------------Affected Tables-------------------------------------    
 --------V3M_FIN_MstInvoice    
 --------V3M_FIN_DetInvoice    
 --------V3M_FIN_StudentLedger    
 --------V3M_FIN_FeeHeadLedger    
 ---------------------------------------------------------------    
 SET @Result = NULL
 BEGIN TRY
 BEGIN TRANSACTION

	-------------------------------Getting Challan Id----------------------------------    
	Select @ChallanNo=InvoiceNo,@GroupCode=GroupCode,@BranchCode=BranchCode,@SessionId=SessionId,@ChllanGeneratedDate=CreatedDate,@ClassCode=ClassCode from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId    
	--------------------------------Getting FeeHead-----------------------------------    
	--Select @HeadAmount=Amount,@Concession=Concession,@ReceivedAmt=ReceivedAmt,@ReceivedDate=ReceivedDate    
	--from V3M_FIN_DetInvoice where InvoiceId=@InvoiceId and MonthNo=@MonthNo    
	---------------------------------Getting Fee Head Name-------------------------------    
	Select @FeeHeadName=FeeHeadName,@DisplayOrder=DisplayOrder from V3M_FIN_MstFeeHead where FeeHeadId=@HeadId and GroupCode=@GroupCode and BranchCode=@BranchCode    
    
	--================================================================Checking Concession===============================================================    
    
	Select @tmpConcessionId=ConcessionId from [dbo].[V3M_FIN_DetConcessionMapToStudent] where Status=1 and GroupCode=@GroupCode and BranchCode=@BranchCode and SessionId=@SessionId and StudentId=@StudentId    
    
    DECLARE @ConcessionName varchar(50)='',@ConcessionType varchar(50)='',@ConcessionAmount decimal(18,2)=0    
    SELECT @ConcessionName = Concession,@ConcessionType=Type  FROM V3M_FIN_MstConcession WHERE ConcessionId = @tmpConcessionId    
	--Change against Ticket id 54770
	if(@ConcessionName = 'Other' or @ConcessionType ='Student')    
	begin    
		select @ConcessionAmount = case ConcessionType when 0 then (@HeadAmount * ConcessionValue)/100 else ConcessionValue end     
		from V3M_FIN_DetMapStudentFeeHeadConcession     
		where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and StudentId  = @StudentId and     
		ClassCode = @ClassCode and IsValid = 1 and FeeHeadId = @HeadId     
	end    
	else    
	begin     
		select @ConcessionAmount = case ConcessionType when 0 then (@HeadAmount * ConcessionApplicable)/100 else ConcessionApplicable end      
		from V3M_FIN_DetCocessionMapToFeeHead where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and     
		ConcessionId = @tmpConcessionId and FeeHeadId = @HeadId and IsValid = 1    
	end    
    
    --==================================================================================================================================    

	-------------------------------------------------------Check for Concession Round Off-----------------------------------------------------
	DECLARE @IsRoundOff int=0
	if exists(select KeyValue from DefaultSettings where GroupCode=@GroupCode and BranchCode=@BranchCode and KeyName='ConcessionRoundOff')
	begin
	select @IsRoundOff=KeyValue from DefaultSettings where GroupCode=@GroupCode and BranchCode=@BranchCode and KeyName='ConcessionRoundOff'
	if(@IsRoundOff=1)
		set @ConcessionAmount=ROUND(@ConcessionAmount,0)
	else if(@IsRoundOff=2)
		set @ConcessionAmount=FLOOR(@ConcessionAmount)
	else if(@IsRoundOff=3)
		set @ConcessionAmount=CEILING(@ConcessionAmount)
	end

	--------------------------------------------------------------------		   

		if(@ConcessionAmount > @HeadAmount)
		begin
			ROLLBACK TRANSACTION;
			BEGIN TRANSACTION ;

			insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)        
			values('Finance','ViewStudentChallan.aspx','Proc FIN_Usp_AddFeeHeadToStudentChallan','In Add Fee Head Concessoin amt can not be > Fee Head Amt. For InvoiceId='+cast(@InvoiceId as Varchar),GETDATE())        
			
			set @Result = -11  
		end
		ELSE
		BEGIN
        
			--------------------------------Updating V3M_FIN_MstInvoice -----------------------    
			Update V3M_FIN_MstInvoice Set TotalAmount=TotalAmount+@HeadAmount,TotalConcession=TotalConcession+@ConcessionAmount    
			where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode    

			-----------------------------------------------------updating V3M_FIN_MstInvoice Payment Status--------------------------------        
			DECLARE @tmpchBalance decimal(18,2)=0        
			select @tmpchBalance=ReceivedAmount from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode       
	        
			if(@tmpchBalance>0)        
			begin        
				Update V3M_FIN_MstInvoice set Ch_Status=2 where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode           
			end  
			else       
			begin        
				Update V3M_FIN_MstInvoice set Ch_Status=1 where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode          
			end           
          

			--------------------------------Updating V3M_FIN_DetInvoice -----------------------    
			DECLARE @DetInvoiceId int =0    
			Select @DetInvoiceId=isnull(Max(DetInvoiceId),0)+1 from V3M_FIN_DetInvoice    
    
			insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)    
			values(@DetInvoiceId,@InvoiceId,@HeadId,@DisplayOrder,@MonthNo,@HeadAmount,@ConcessionAmount,0,0,0,1,1)    
			------------------------------------------------------------------------------------------------------------------------------    
			set @Narration='Added fee head = '+@FeeHeadName+',  Month = '+ Cast(DateName(mm,DATEADD(mm,@MonthNo-1,0)) as varchar)+', session='+@SessionId+' Challan No= '+Cast(@ChallanNo as varchar)+'. '+@Narration1    
			--------------------------------- Entry into V3M_FIN_StudentLedger-------------    
			insert into V3M_FIN_StudentLedger(GroupCode,BranchCode,SessionId,StudentId,ReferenceNo,ReferenceType,Narration,Amount,TransMode,TransDetail,RunningBalance,TransactionDate,TransactionBy)    
			Values(@GroupCode,@BranchCode,@SessionId,@StudentId,@ChallanNo,'CHALLAN',@Narration,@HeadAmount,'','',dbo.Fn_GetStudentRunningBalance(@GroupCode,@BranchCode,@SessionId,@StudentId,@HeadAmount),getdate(),@CreatedBy)    
    
			---------------------------------Entry into V3M_FIN_FeeHeadLedger-------------   
			--Rakesh FeeHeadLedger verification
			insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
			,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)    
			Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@MonthNo,Month(getdate()),@SessionId,@HeadAmount,'STUDENT',@StudentId,'DUE',@Narration
			,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_AddFeeHeadToStudentChallan')    
    
    
			if(@ConcessionAmount>0)    
			begin    
				set @Narration='Concession added fee head = '+@FeeHeadName+',  Month = '+ Cast(DateName(mm,DATEADD(mm,@MonthNo-1,0)) as varchar)+', session='+@SessionId+' Challan No= '+Cast(@ChallanNo as varchar)+'. '+@Narration1    
   
				--------------------------------- Entry into V3M_FIN_StudentLedger-------------    
				insert into V3M_FIN_StudentLedger(GroupCode,BranchCode,SessionId,StudentId,ReferenceNo,ReferenceType,Narration,Amount,TransMode,TransDetail,RunningBalance,TransactionDate,TransactionBy)  
				Values(@GroupCode,@BranchCode,@SessionId,@StudentId,@ChallanNo,'CHALLAN',@Narration,-@ConcessionAmount,'','',dbo.Fn_GetStudentRunningBalance(@GroupCode,@BranchCode,@SessionId,@StudentId,-@ConcessionAmount),getdate(),@CreatedBy)    
    
				---------------------------------Entry into V3M_FIN_FeeHeadLedger------------- 
				--Rakesh FeeHeadLedger verification
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
				,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)    
				Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@MonthNo,Month(getdate()),@SessionId,-@ConcessionAmount,'STUDENT',@StudentId,'CON',@Narration
				,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_AddFeeHeadToStudentChallan')    
			end
			
		END
    
 COMMIT TRANSACTION
 -- keep -11 (concession > fee head amount); success only when nothing was rejected
 if(ISNULL(@Result,0) <> -11)
	set @Result=1
 SELECT  @ErrorCode  = 0
 RETURN @ErrorCode                               -- =0 if success,  <>0 if failure

 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT > 0
	ROLLBACK TRANSACTION
  set @Result=0                       -- =0 if success,  <>0 if failure
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
  values('Proc','EditChall_AddFeeHead','FIN_Usp_AddFeeHeadToStudentChallan',ERROR_MESSAGE(), GETDATE())
  RETURN @ErrorCode
 END CATCH
End    
    
    
    
/*    
    
    
DECLARE @Result int=0    
exec FIN_Usp_AddFeeHeadToStudentChallan 'S0170170086','98',2,6,2500,'ravi',@Result output    
select @Result    
    
    
*/
GO

