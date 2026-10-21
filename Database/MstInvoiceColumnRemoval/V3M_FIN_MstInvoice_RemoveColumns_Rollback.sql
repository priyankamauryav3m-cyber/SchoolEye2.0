-- =====================================================================================================
-- ROLLBACK for V3M_FIN_MstInvoice_RemoveColumns_AlterProcedures.sql
-- Original definitions of the 14 procedures, taken from the database before the change.
-- NOTE: these originals use ArrearAmount / ActualReceived / LateCharges. They will only compile if those
--       columns are first added back to V3M_FIN_MstInvoice. Keep this file as a backup / reference.
-- =====================================================================================================
SET XACT_ABORT ON;
SET NOEXEC OFF;
GO
BEGIN TRANSACTION;
GO

-- USP_FINGetStudentBalanceList
---- Exec USP_FINGetStudentBalanceList '01','01',1043,0,'','','',0,'' 
ALTER PROCEDURE [dbo].[USP_FINGetStudentBalanceList]  
(  
@GroupCode  VARCHAR(20),@BranchCode VARCHAR(20),@SessionId  BIGINT,@FeeTemplateId int=0,@ClassCode varchar(max)='',  
@SectionId VARCHAR(100) = '',@PeriodType varchar(20)='',@PeriodId int=0,@InvType int=0,@StudentNo varchar(50)=''  
)  
AS  
BEGIN  
    SELECT vw.StudentId,vw.StudentName,vw.SessionId,sm.ControlNo,sm.ImagePath,vw.StudentNo,vw.ClassSection,  
    vw.AdmissionNo,MT.FeeTemplateId As TemplateId , MT.TemplateName,FIN.DueDate,CONVERT(DATE, FIN.CreatedDate) AS InvoiceDate ,  
    FIN.InvoiceId,FIN.InvoiceNo,FIN.PeriodId,FEPE.PeriodName ,FIN.InvType,FIN.TotalAmount,FIN.TotalConcession,FIN.ReceivedAmount,FIN.ArrearAmount,vw.IsTransportRequired,
    FIN.Ch_Status as Status,FIN.IsActive FROM vwStudentDetails vw WITH (NOLOCK)      
    INNER JOIN V3M_FIN_MstFeeTemplate MT WITH (NOLOCK) ON MT.FeeTemplateId = vw.FeeTemplateId  
    AND MT.GroupCode = vw.GroupCode AND MT.BranchCode = vw.BranchCode   AND MT.IsValid = 1 
    INNER JOIN StudentMaster Sm WITH (NOLOCK)  ON Sm.StudentId = vw.StudentId  
    AND Sm.GroupCode = vw.GroupCode AND Sm.BranchCode = vw.BranchCode AND Sm.SessionId = vw.SessionId  
     inner join V3M_FIN_MstInvoice FIN with(nolock) on FIN.StudentId =vw.StudentId 
     INNER JOIN V3M_FIN_Mst_FeeCollectionPeriodConfig FEPE WITH (NOLOCK)  ON FEPE.PeriodId = FIN.PeriodId
     And FEPE.SessionId=FIN.SessionId
     WHERE vw.GroupCode = @GroupCode AND vw.BranchCode = @BranchCode AND vw.SessionId = @SessionId  
      And (@FeeTemplateId =0 OR vw.FeeTemplateId=@FeeTemplateId )  
      AND (ISNULL(@ClassCode,'') = '' OR vw.ClassCode IN (SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@ClassCode, ',') ) )  
      AND (ISNULL(@SectionId,'') = '' OR vw.SectionId IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@SectionId, ',') ) )  
      And (@StudentNo ='' OR  @StudentNo Is Null OR sm.ControlNo=@StudentNo )  
      And (@PeriodType ='' OR  @PeriodType Is Null OR vw.PeriodType=@PeriodType )  
      And (@PeriodId=0 or  FIN.PeriodId=@PeriodId)  
       And (@InvType=0 or   FIN.InvType=@InvType)  
    ORDER BY  vw.StudentNo;  
END  
  
--select PeriodType, * from vwStudentDetails  
--select * from vwStudentDetails  
--select * from StudentMaster where ControlNo='CON0051'  

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at USP_FINGetStudentBalanceList - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- USP_GetStudentChallanDetailsForEdit
ALTER PROCEDURE [dbo].[USP_GetStudentChallanDetailsForEdit]
(
    @InvoiceId BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    ------------------------------------------------------------
    -- Result Set 1 : Challan Master
    ------------------------------------------------------------
    SELECT
        VT.StudentNo,VT.ControlNo,VT.StudentName,VT.RollNo,VT.ClassCode,VT.ClassName,VT.ClassOrder,VT.SectionId,VT.SectionName,
        ISNULL(MC.InvoiceNo,'N/A') AS InvoiceNo,
        MC.InvoiceId,
        CONVERT(VARCHAR(10),MC.CreatedDate,103) AS CreatedDate,
        CONVERT(VARCHAR(10),MC.DueDate,103) AS DueDate,
        MC.TotalAmount + MC.ArrearAmount AS TotalAmount,MC.ArrearAmount,MC.TotalConcession,
        --(MC.ReceivedAmount + MC.AdjArrear) AS ReceivedAmount,
        MC.LateCharges,MC.CH_Status,MC.SessionId,MC.GroupCode,MC.BranchCode,P.PeriodName,VT.StudentFeeTemplateId
    FROM vwStudentDetails VT INNER JOIN V3M_FIN_MstInvoice MC
            ON VT.StudentId = MC.StudentId AND VT.SessionId = MC.SessionId
        LEFT JOIN V3M_FIN_Mst_FeeCollectionPeriodConfig P ON P.PeriodId = MC.PeriodId
    WHERE MC.InvoiceId = @InvoiceId
    ORDER BY VT.ClassOrder,VT.SectionName,VT.RollNo,VT.StudentName;
    ------------------------------------------------------------
    -- Result Set 2 : Challan Details
    ------------------------------------------------------------
    SELECT
       DCN.FeeHeadId, MFH.FeeHeadName, CASE WHEN DCN.MonthNo = 0 THEN '' ELSE 
       LEFT(DATENAME(MONTH, DATEFROMPARTS(2015,DCN.MonthNo,1)),3) END AS MonthDisplay,
        DCN.MonthNo,DCN.Amount,ISNULL(DCN.WaiveOff,0) AS WaiveOff,
        DCN.Concession,DCN.ReceivedAmt,
        ISNULL(DCN.Amount,0) - (ISNULL(DCN.Concession,0) + ISNULL(DCN.ReceivedAmt,0) + ISNULL(DCN.WaiveOff,0)) AS Payable,
        DCN.ReceivedDate
    FROM V3M_FIN_MstInvoice MC INNER JOIN V3M_FIN_DetInvoice DCN
            ON MC.InvoiceId = DCN.InvoiceId
        INNER JOIN V3M_FIN_MstFeeHead MFH ON DCN.FeeHeadId = MFH.FeeHeadId
        INNER JOIN V3M_FIN_Mst_MonthDisplayOrder MDO ON MC.GroupCode = MDO.GroupCode
           AND MC.BranchCode = MDO.BranchCode AND DCN.MonthNo = MDO.MonthNo
    WHERE MC.InvoiceId = @InvoiceId AND MC.CH_Status > 0 AND DCN.Status > 0
    ORDER BY MFH.DisplayOrder;
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at USP_GetStudentChallanDetailsForEdit - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- Usp_GenerateStudentChallan

ALTER PROC [dbo].[Usp_GenerateStudentChallan] @GroupCode varchar(20),@BranchCode varchar(20),@SessionId bigint,@ClassCode VARCHAR(50),@SectionId VARCHAR(50),  
@PeriodType varchar(20),  
@PeriodId int ,@StudentId bigint,@CreatedBy varchar(50),@SocietyId int=0  
AS  
DECLARE @DueToDate date  
DECLARE @ErrorCode  int=0    
DECLARE @BankId int=0  
DECLARE @AccountNO varchar(50)=0  
DECLARE @FeeDueDate datetime,@admDate datetime  
DECLARE @ArrearAmount decimal(18,2)=0  
DECLARE @StudentBalanceAmount decimal(18,2)=0  
BEGIN  
 BEGIN TRY  
    BEGIN TRANSACTION  
   --===========================================Getting Challan Bank Details============================  
     
   Select @BankId=BA.BankId,@AccountNO=BA.AccountNo   
   From V3M_FIN_MstBank as MB  
   inner join V3M_FIN_MstBankAccount BA  
   on MB.BankId =BA.BankId   and MB.GroupCode COLLATE DATABASE_DEFAULT=BA.GroupCode  COLLATE DATABASE_DEFAULT 
   where BA.IsValid=1 and  BA.GroupCode=@GroupCode and BA.BranchCode=@BranchCode and BA.SocietyId=@SocietyId and BA.ForFee=1  
     
    print 'Mahesh'  
  
   select @FeeDueDate=FeeDueDate from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId  
   select top 1 @admDate = cast(AdmissionDate as date)  from studentmaster where GroupCode=@GroupCode and BranchCode=@BranchCode and ClassCode=@ClassCode and StudentId=@SessionId  
   if(cast(@admDate as date) >cast(@FeeDueDate as date) )  
    set @FeeDueDate = @admDate  
  
   --===========================================Getting Student==========================================  
   IF(@StudentId<>'0')  
    DECLARE CURR_STUDENT CURSOR FOR  
    select StudentNo from studentmaster where GroupCode=@GroupCode and BranchCode=@BranchCode and ClassCode=@ClassCode and IsValid=1 and PeriodType=@PeriodType and StudentId=@StudentId  
   ELSE IF(@SectionId<>'0')  
    DECLARE CURR_STUDENT CURSOR FOR  
    select StudentNo from studentmaster where GroupCode=@GroupCode and BranchCode=@BranchCode and SessionId = @SessionId and ClassCode=@ClassCode and SectionId=@SectionId and IsValid=1 and PeriodType=@PeriodType  
   ELSE  
    DECLARE CURR_STUDENT CURSOR FOR  
    select StudentNo from studentmaster where GroupCode=@GroupCode and BranchCode=@BranchCode and SessionId = @SessionId and ClassCode=@ClassCode and IsValid=1 and PeriodType=@PeriodType  
  
   OPEN CURR_STUDENT  
   FETCH FROM CURR_STUDENT INTO @StudentId  
   WHILE @@FETCH_STATUS=0  
   BEGIN  
    print 'Mahesh1'  
    if not exists(select * from V3M_FIN_MstInvoice where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and StudentId = @StudentId and PeriodId = @PeriodId and Ch_Status > 0)  
    begin  
    print 'Mahesh'  
     set @ArrearAmount = 0  
     --=====================================Creating #Table for Result==================================================================  
     create table #tblChallanDetails(FeeHeadId int, HeadName varchar(50), HeadAmount decimal(18,2), ConcessionAmount decimal(18,2), AllMonthsDisplay varchar(100),    
     AllMonths varchar(50), TotalConcession decimal(18,2), TotalAmount decimal(18,2), DisplayOrder int, HeadType varchar(50), FeeType varchar(50), NoMonth INT DEFAULT 0 )     
     --=================================================================================================================================  
     insert into #tblChallanDetails  
     Exec V3M_FIN_UspGetFeeDetailsForChallan @GroupCode,@BranchCode, @SessionId,@ClassCode,@StudentId,@PeriodId  
     --select * from #tblChallanDetails  
     --===================================Createing Challan===================================================================  
     DECLARE @InvoiceId bigint,@InvoiceNo varchar(20),@ChallanTotal decimal(18,2),@TotalConcession decimal(18,2)  
     set @InvoiceId=0  
     set @InvoiceNo=0  
     set @ChallanTotal=0  
     set @TotalConcession=0  
     set @ArrearAmount = 0  
  
     if exists(select * from #tblChallanDetails)  ----------if Challan Details not exists then no need to generate Challan  
     begin  
  
      -----------------------------------------------Getting total amount---------------------------------------------------  
      select @ChallanTotal=isnull(sum(TotalAmount),0),@TotalConcession=isnull(sum(TotalConcession),0) from #tblChallanDetails  
      
      --===========================================Calculating Arrier Amount================================================  
      select @ArrearAmount= dbo.Fn_GetStudentBalance(@Groupcode, @BranchCode, @SessionId, @StudentId) -- done on 01/10/2015  
      Set @ArrearAmount=isnull(@ArrearAmount,0)  
      --========================================================================================================================  
      IF NOT EXISTS(              
        select InvoiceNo  
        from V3M_FIN_MstInvoice  as MC  
        inner join V3M_FIN_Mst_FeeCollectionPeriodConfig as FCP  
        on MC.PeriodId=FCP.PeriodId  
        where MC.Ch_Status>0 and MC.GroupCode=@GroupCode and MC.BranchCode=@BranchCode  
        and MC.SessionId=@SessionId and MC.StudentId=@StudentId and MC.ClassCode=@ClassCode and MC.PeriodId>=@PeriodId and   
        FCP.PeriodType=@PeriodType  
        )   
      BEGIN  
        ---------------------Getting challan no and ID------------------------------  
        --exec V3M_Fin_StudentChallanNoPattern @GroupCode,@BranchCode,@SessionName,@ChallanNo output  
          EXEC UspGenerateID @GroupCode, @BranchCode,'', @SessionId,'StudentChallan', @InvoiceNo output
        Select @InvoiceId=isnull(Max(InvoiceId),0)+1 from V3M_FIN_MstInvoice  
        -------------------------------------------------------------------------------------  
       if exists(select * from V3M_FIN_MstInvoice where groupcode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId  
       and IsActive=1)  
       BEGIN  
        --=====================================Select previous actvie challan details=============================================================================  
        DECLARE @ActiveChalllanId bigint,@ActvChallanNo varchar(50),@ActvchAmount decimal,@AcvtivePeriodId int  
        select @ActiveChalllanId=InvoiceId,@ActvchAmount=(TotalAmount+ArrearAmount+LateCharges)-(TotalConcession+ActualReceived),@ActvChallanNo=InvoiceNo,  
         @AcvtivePeriodId=PeriodId from V3M_FIN_MstInvoice where groupcode=@GroupCode and BranchCode=@BranchCode --and SessionName=@SessionName   
         and StudentId=@StudentId and IsActive=1  
        
        Update V3M_FIN_MstInvoice set IsActive=0 where GroupCode=@GroupCode and BranchCode=@BranchCode   
        and StudentId=@StudentId and InvoiceId!=@InvoiceId  
       END  
       --========================================================================================================  
  
       --select @ChallanId,@GroupCode,@BranchCode,@SessionName,@StudentNo,@ClassCode,@ChallanNo,@PeriodId,@ChallanTotal,@TotalConcession,0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0  
       insert into V3M_FIN_MstInvoice(InvoiceId,GroupCode,BranchCode,SessionId,StudentId,ClassCode,InvoiceNo,PeriodId,TotalAmount,TotalConcession,  
        ReceivedAmount,ArrearAmount,IsActive,Ch_Status,BankId,AccountNo,CretaedBy,CreatedDate,DueDate,ActualReceived,SocietyId)   
       values(@InvoiceId,@GroupCode,@BranchCode,@SessionId,@StudentId,@ClassCode,@InvoiceNo,@PeriodId,@ChallanTotal,@TotalConcession,  
        0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0,@SocietyId)  
       --============================================Recording Student Transcation for creating challan===================================  
       DECLARE @Narration VARCHAR(150),@TransDate datetime,@Result int,@tmpChallanTotal decimal,@PeriodName varchar(50)  
       select @PeriodName=PeriodName from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId  
       Set @Narration='Challan for '+@PeriodName+'/dated:'+convert(varchar,getdate(),103)  
       set @TransDate=getdate()  
       --set @tmpChallanTotal=(@ChallanTotal+@ArrearAmount)-@TotalConcession  
       set @tmpChallanTotal=@ChallanTotal-@TotalConcession  -- done on 01/10/2015  
       exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@SessionId,@StudentId,@InvoiceNo,'CHALLAN',@Narration,@tmpChallanTotal,'','',@TransDate,@CreatedBy,@Result output  
   
       --===================================Creating cursor for generated fee head===============================================  
       DECLARE @CH_FeeHeadId int,@CH_HeadAmount DECIMAL(18,2),@AllMonths INT,@DisplayOrder INT,@ConcessionAmt Decimal(18,2)  
  
       DECLARE CURR_CHALLAN CURSOR FOR  
        select FeeHeadId,HeadAmount,AllMonths,DisplayOrder,TotalConcession from #tblChallanDetails  
       OPEN CURR_CHALLAN  
       FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt  
       WHILE @@FETCH_STATUS=0  
       BEGIN  
        Declare @DetInvoiceId bigint  
        Select @DetInvoiceId=isnull(Max(DetInvoiceId),0)+1 from V3M_FIN_DetInvoice  
        if(@CH_HeadAmount = @ConcessionAmt)  
        begin  
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status,ReceivedDate)  
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,1, getdate())  
        end  
        else  
        begin  
  
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)  
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,1)  
        end  
         
        --Rakesh FeeHeadLedger verification  
        insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,  
         Narration,IsValid,InvoiceId,TransDate,TransBy,CRDR,EnteredVia)  
        values(@GroupCode, @BranchCode, @SessionId, @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, @CH_HeadAmount,'STUDENT', @StudentId, 'DUE',   
         @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_Usp_GenerateStudentChallan')  
        if @ConcessionAmt > 0   
        begin    
         --Rakesh FeeHeadLedger verification  
         insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,  
          Narration,IsValid,InvoiceId,TransDate,TransBy, CRDR,EnteredVia)  
         values(@GroupCode, @BranchCode, @SessionId, @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, - @ConcessionAmt,'STUDENT', @StudentId, 'CON',   
          @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy, 'DR','Pro_Usp_GenerateStudentChallan')  
        end  
        FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt  
       END  
       CLOSE CURR_CHALLAN   
       DEALLOCATE CURR_CHALLAN  
       --=========================================================================================================================  
       drop table #tblChallanDetails  
      end  
     end  
    END  
    --exec V3M_FIN_UspAdjustChallanFromAdvanceAmount @GroupCode, @BranchCode, @SessionName, @StudentNo, @CreatedBy  
    FETCH FROM CURR_STUDENT INTO @StudentId   
   END  
   CLOSE CURR_STUDENT   
   DEALLOCATE CURR_STUDENT  
      Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @StudentId,'StudentChallan'
 COMMIT TRANSACTION  
 SELECT  @ErrorCode  = 0  
 RETURN @ErrorCode                               -- =0 if success,  <>0 if failure  
  
 END TRY  
 BEGIN CATCH  
  ROLLBACK TRANSACTION  
  RETURN @ErrorCode                               -- =0 if success,  <>0 if failure  
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)  
  values('Finance','GenerateChallan','Usp_GenerateStudentChallanXX',@@ERROR, GETDATE())  
 END CATCH  
END   
  
/*  
 exec Usp_GenerateStudentChallan '01','01',1043,'Class1,Class2,Class3,Class4','0','Quarterly',3,40041,'Priya',0  
*/  
  

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at Usp_GenerateStudentChallan - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- Usp_GenerateStudentChallanWithTemplate
---- exec Usp_GenerateStudentChallanWithTemplate '01','01',1043,'Class1',18,'Quarterly',1,80050,'V3M',0   

ALTER PROC [dbo].[Usp_GenerateStudentChallanWithTemplate] @GroupCode varchar(20),@BranchCode varchar(20),    
@SessionId bigint,@ClassCode VARCHAR(50),@SectionId int,@PeriodType varchar(20),    
@PeriodId int ,@StudentId bigint ,@CreatedBy varchar(50),@SocietyId int=0  
AS    
 DECLARE @DueToDate date    
 DECLARE @ErrorCode  int=0      
 DECLARE @BankId int=0    
 DECLARE @AccountNO varchar(50)=''    
 DECLARE @FeeDueDate datetime,@admDate datetime    
 DECLARE @ArrearAmount decimal(18,2)=0    
 DECLARE @StudentBalanceAmount decimal(18,2)=0  
 Declare  @StudentChalanNo  varchar(50)     
 Declare  @AdjArrear varchar(50)='0'  
BEGIN    
 BEGIN TRY    
  BEGIN TRANSACTION    
  --===========================================Getting Challan Bank Details============================    
  Select @BankId=BA.BankId,@AccountNO=BA.AccountNo     
  From V3M_FIN_MstBank as MB    
  inner join V3M_FIN_MstBankAccount BA    
  on MB.BankId =BA.BankId and MB.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS =BA.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS    
  where BA.IsValid=1 and  BA.GroupCode=@GroupCode and BA.BranchCode=@BranchCode and BA.ForFee=1 and BA.SocietyId=@SocietyId    
  if(@SectionId =0)    
   set @SectionId = 0;    
  select @FeeDueDate=FeeDueDate from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId    
    
  --DECLARE @StudentFeeTemplate int=0    
  --select top 1 @admDate = cast(AdmissionDate as date),@StudentFeeTemplate=sj.FeeTemplateId  from studentmaster sm     
  --inner join StudentJourney sj on sj.GroupCode=sm.GroupCode and sj.BranchCode=sm.BranchCode and sj.SessionName=sm.SessionName and sj.StudentNo=sm.StudentNo    
  --where sm.GroupCode=@GroupCode and sm.BranchCode=@BranchCode and sm.StudentNo=@StudentNo and sm.SessionName = @SessionName    
    

  DECLARE @StudentFeeTemplate INT = 0;    
SELECT TOP 1 @admDate = CAST(AdmissionDate AS DATE),@StudentFeeTemplate = sj.FeeTemplateId  FROM studentmaster sm    
INNER JOIN StudentJourney sj     
    ON sj.GroupCode COLLATE DATABASE_DEFAULT = sm.GroupCode COLLATE DATABASE_DEFAULT    
    AND sj.BranchCode COLLATE DATABASE_DEFAULT = sm.BranchCode COLLATE DATABASE_DEFAULT    
    AND sj.SessionId  = sm.SessionId  
    AND sj.StudentId  = sm.StudentId   
WHERE sm.GroupCode = @GroupCode     
    AND sm.BranchCode = @BranchCode     
    AND sm.StudentId = @StudentId     
    AND sm.SessionId = @SessionId;    
    
  if(cast(@admDate as date) >cast(@FeeDueDate as date) )    
   set @FeeDueDate = @admDate    
       
   --Declare @StudentID varchar(50) = @StudentId    
   --DECLARE CUR_STUDENT CURSOR FOR    
   -- select StudentId from studentmaster where GroupCode=@GroupCode and BranchCode=@BranchCode and SessionId = @SessionId and ClassCode=@ClassCode     
   -- and PeriodType=@PeriodType and IsValid=1     
   -- and (StudentId=@StudentId  or @StudentId = 0)     
   -- and (SectionId=@SectionId or  @SectionId =0)   
   --OPEN CUR_STUDENT    
   --FETCH  FROM CUR_STUDENT INTO @StudentId    
   --WHILE @@FETCH_STATUS=0    
  
   DECLARE CUR_STUDENT CURSOR LOCAL FAST_FORWARD FOR  SELECT StudentId FROM studentmaster WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND ClassCode = @ClassCode AND PeriodType = @PeriodType AND IsValid = 1 AND (StudentId = @StudentId OR @StudentId = 0)AND (SectionId = @SectionId OR @SectionId = 0) OPEN CUR_STUDENT FETCH NEXT FROM CUR_STUDENT INTO @StudentId WHILE @@FETCH_STATUS = 0  
   BEGIN   
    if(@StudentFeeTemplate = 0)    
    begin    
     SELECT  @ErrorCode  = -11                             -- = -11 Fee Template not mapped with student    
     insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)    
     values('Finance','GenerateChallan','Usp_GenerateStudentChallanWithTemplate','Fee Template Not Mapped - '+@StudentId, GETDATE())    
    end    
    else    
    begin    
     if not exists(select * from V3M_FIN_MstInvoice where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and StudentId = @StudentId and PeriodId = @PeriodId and Ch_Status > 0)    
     begin    
      set @ArrearAmount = 0    
      --=====================================Creating #Table for Result==================================================================    
      create table #tblChallanDetails(FeeHeadId int, HeadName varchar(50), HeadAmount decimal(18,2), ConcessionAmount decimal(18,2), AllMonthsDisplay varchar(100),      
      AllMonths varchar(50), TotalConcession decimal(18,2), TotalAmount decimal(18,2), DisplayOrder int, HeadType varchar(50), FeeType varchar(50), NoMonth INT DEFAULT 0 )       
      --=================================================================================================================================    
      insert into #tblChallanDetails    
      ---Exec V3M_FIN_UspGetFeeDetailsForChallanWithTemplate @GroupCode,@BranchCode, @SessionId,@ClassCode,@StudentId,@PeriodId   
	  
	  Exec V3M_FIN_UspGetFeeDetailsForInvoiceWithTemplateNilesh @GroupCode,@BranchCode, @SessionId,@ClassCode,@StudentId,@PeriodId   
      --select * from #tblChallanDetails    
      --===================================Createing Challan===================================================================    
      DECLARE @InvoiceId bigint,@InvoiceNo varchar(20),@ChallanTotal decimal(18,2),@TotalConcession decimal(18,2)    
      set @InvoiceId=0    
      set @InvoiceNo=0    
      set @ChallanTotal=0    
      set @TotalConcession=0    
      set @ArrearAmount = 0    
      -----------------------------------------------Getting total amount---------------------------------------------------    
      select @ChallanTotal=isnull(sum(TotalAmount),0),@TotalConcession=isnull(sum(TotalConcession),0) from #tblChallanDetails    
        
      --===========================================Calculating Arrier Amount================================================    
      select @ArrearAmount= dbo.Fn_GetStudentBalance(@Groupcode, @BranchCode, @SessionId, @StudentId)     
      Set @ArrearAmount=isnull(@ArrearAmount,0)    
         
      --========================================================================================================================    
      IF NOT EXISTS( select @InvoiceNo from V3M_FIN_MstInvoice  as MC inner join V3M_FIN_Mst_FeeCollectionPeriodConfig as FCP    
      on MC.PeriodId=FCP.PeriodId    
      where MC.Ch_Status>0 and MC.GroupCode=@GroupCode and MC.BranchCode=@BranchCode    
      and MC.SessionId=@SessionId and MC.StudentId=@StudentId and MC.PeriodId>=@PeriodId and FCP.PeriodType=@PeriodType    
      --and MC.ClassCode=@ClassCode     f
      )     
      BEGIN    
       ---------------------Getting challan no and ID------------------------------    
       --exec V3M_Fin_StudentChallanNoPattern @GroupCode,@BranchCode,@SessionName,@ChallanNo output    
              --EXEC UspGenerateID @GroupCode, @BranchCode,'', @SessionId,'StudentChallan', @InvoiceNo output  
      --Select @InvoiceId=isnull(Max(@InvoiceId),0)+1 from V3M_FIN_MstInvoice    

     EXEC UspGenerateID @GroupCode, @BranchCode,'', @SessionId,'StudentChallan', @InvoiceNo output  
    Select @InvoiceId = ISNULL(MAX(InvoiceId),0) + 1 from V3M_FIN_MstInvoice  
    
       -------------------------------------------------------------------------------------    
       if exists(select * from V3M_FIN_MstInvoice where groupcode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and IsActive=1)    
       BEGIN    
        -----=====================================Select previous actvie challan details=============================================================================    
        --DECLARE @ActiveChalllanId bigint,@ActvChallanNo varchar(50),@ActvchAmount decimal,@AcvtivePeriodId int    
        --select @ActiveChalllanId=ChallanId,@ActvchAmount=(TotalAmount+ArrearAmount+LateCharges)-(TotalConcession+ActualReceived),@ActvChallanNo=ChallanNo,    
        --@AcvtivePeriodId=PeriodId from V3M_FIN_MstChallanNo where groupcode=@GroupCode and BranchCode=@BranchCode --and SessionName=@SessionName     
        --and StudentNo=@StudentID and IsActive=1    
        Update V3M_FIN_MstInvoice set IsActive=0 where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and InvoiceId!=@InvoiceId    
       END    
	   
       --select @ChallanId,@GroupCode,@BranchCode,@SessionName,@StudentID,@ClassCode,@ChallanNo,@PeriodId,@ChallanTotal,@TotalConcession,0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0    
           
       insert into V3M_FIN_MstInvoice(InvoiceId,GroupCode,BranchCode,SessionId,StudentId,ClassCode,InvoiceNo,PeriodId,TotalAmount,TotalConcession,    
        ReceivedAmount,ArrearAmount,IsActive,Ch_Status,BankId,AccountNo,CretaedBy,CreatedDate,DueDate,ActualReceived,FeeTemplateId,SocietyId)     
       values(@InvoiceId,@GroupCode,@BranchCode,@SessionId,@StudentId,@ClassCode,@InvoiceNo,@PeriodId,@ChallanTotal,@TotalConcession,    
        0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0,@StudentFeeTemplate,@SocietyId)    
           
       --============================================Recording Student Transcation for creating challan===================================    
       DECLARE @Narration VARCHAR(150),@TransDate datetime,@Result int,@tmpChallanTotal decimal,@PeriodName varchar(50)    
       select @PeriodName=PeriodName from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId    
           
       Set @Narration='Challan for '+@PeriodName+'/dated:'+convert(varchar,getdate(),103);    
       set @TransDate=getdate();    
          
      --set @tmpChallanTotal=(@ChallanTotal+@ArrearAmount)-@TotalConcession    
       set @tmpChallanTotal=@ChallanTotal-@TotalConcession     
       exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@SessionId,@StudentId,@InvoiceNo,'CHALLAN',@Narration,@tmpChallanTotal,'','',@TransDate,@CreatedBy,@Result output    
     
       --===================================Creating cursor for generated fee head===============================================    
       DECLARE @CH_FeeHeadId int,@CH_HeadAmount DECIMAL(18,2),@AllMonths INT,@DisplayOrder INT,@ConcessionAmt Decimal(18,2)    
    
       DECLARE CURR_CHALLAN CURSOR FOR    
        select FeeHeadId,HeadAmount,AllMonths,DisplayOrder,TotalConcession from #tblChallanDetails    
       OPEN CURR_CHALLAN    
       FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt    
       WHILE @@FETCH_STATUS=0    
       BEGIN    
        Declare @DetInvoiceId bigint    
        Select @DetInvoiceId=isnull(Max(DetInvoiceId),0)+1 from V3M_FIN_DetInvoice    
        if(@CH_HeadAmount = @ConcessionAmt)    
        begin    
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status,ReceivedDate)    
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,3, getdate())    
        end    
        else    
        begin    
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)    
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,1)    
        end    
        --Rakesh FeeHeadLedger verification    
        insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,    
         Narration,IsValid,InvoiceId,TransDate,TransBy,CRDR,EnteredVia)    
        values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, @CH_HeadAmount,'STUDENT', @StudentID, 'DUE',     
         @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_Usp_GenerateStudentChallanWithTemplate')     
        if @ConcessionAmt > 0     
        begin      
         --Rakesh FeeHeadLedger verification    
         insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,    
          Narration,IsValid,InvoiceId,TransDate,TransBy, CRDR,EnteredVia)    
         values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, - @ConcessionAmt,'STUDENT', @StudentID, 'CON',     
          @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy, 'DR','Pro_Usp_GenerateStudentChallanWithTemplate')    
        end    
        FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt    
       END    
       CLOSE CURR_CHALLAN     
       DEALLOCATE CURR_CHALLAN    
       --=========================================================================================================================    
       drop table #tblChallanDetails    
      end     
     END    
     -------------------exec V3M_FIN_UspAdjustChallanFromAdvanceAmount @GroupCode, @BranchCode, @SessionName, @StudentID, @CreatedBy    
    end    
    FETCH FROM CUR_STUDENT INTO @StudentID     
   END    
   CLOSE CUR_STUDENT     
   DEALLOCATE CUR_STUDENT    
      Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @SessionId,'StudentChallan'  
   COMMIT TRANSACTION    
   SELECT  @ErrorCode  = 0    
   RETURN @ErrorCode                              -- =0 if success,  <>0 if failure    
 END TRY    
  
 BEGIN CATCH    
  ROLLBACK TRANSACTION    
  print ERROR_Message()    
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)    
  values('Finance','GenerateChallan','Usp_GenerateStudentChallanWithTemplate',@@ERROR, GETDATE())    
  RETURN @ErrorCode                               -- =0 if success,  <>0 if failure    
 END CATCH    
END     

/*    
-- exec Usp_GenerateStudentChallanWithTemplate '01','01',1043,'Class1',18,'Quarterly',1,80050,'V3M',0  
Select * from V3M_FIN_MstInvoice
Select * from V3M_Fin_DetInvoice
Select * from V3M_FIN_FeeHeadLedger
truncate table  V3M_FIN_MstInvoice
truncate table  V3M_Fin_DetInvoice
truncate table  V3M_FIN_FeeHeadLedger



*/    
  
  
  
  
  

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at Usp_GenerateStudentChallanWithTemplate - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- Usp_GenerateStudentChallanForAllClassWithTemplateDemoForStudent

--exec Usp_GenerateStudentChallanForAllClassWithTemplateDemoForStudent '01','01',1043,'Class1',18,'Quarterly',1,80050,'V3M',0  
ALTER PROC [dbo].[Usp_GenerateStudentChallanForAllClassWithTemplateDemoForStudent] @GroupCode varchar(20),@BranchCode varchar(20),    
@SessionId bigint,@SectionId int,@PeriodType varchar(20),@SelectedClasses varchar(MAX)=null,    
@PeriodId int ,@StudentId bigint ,@CreatedBy varchar(50),@SocietyId int=0  
AS    
 DECLARE @DueToDate date    
 DECLARE @ErrorCode  int=0      
 DECLARE @BankId int=0    
 DECLARE @AccountNO varchar(50)=''    
 DECLARE @FeeDueDate datetime,@admDate datetime    
 DECLARE @ArrearAmount decimal(18,2)=0    
 DECLARE @StudentBalanceAmount decimal(18,2)=0  
 Declare  @StudentChalanNo  varchar(50)     
 Declare  @AdjArrear varchar(50)='0' ,@ClassCode varchar(MAX)=null
 
BEGIN    
 BEGIN TRY    
  BEGIN TRANSACTION    
  --===========================================Getting Challan Bank Details============================    
  Select @BankId=BA.BankId,@AccountNO=BA.AccountNo     
  From V3M_FIN_MstBank as MB    
  inner join V3M_FIN_MstBankAccount BA    
  on MB.BankId =BA.BankId and MB.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS =BA.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS    
  where BA.IsValid=1 and  BA.GroupCode=@GroupCode and BA.BranchCode=@BranchCode and BA.ForFee=1 and BA.SocietyId=@SocietyId    
  if(@SectionId =0)    
   set @SectionId = 0;    
  select @FeeDueDate=FeeDueDate from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId    

  DECLARE @StudentFeeTemplate INT = 0;    
  SELECT TOP 1 @admDate = CAST(AdmissionDate AS DATE),@StudentFeeTemplate = sj.FeeTemplateId  FROM studentmaster sm    
  INNER JOIN StudentJourney sj     
    ON sj.GroupCode COLLATE DATABASE_DEFAULT = sm.GroupCode COLLATE DATABASE_DEFAULT    
    AND sj.BranchCode COLLATE DATABASE_DEFAULT = sm.BranchCode COLLATE DATABASE_DEFAULT  AND sj.SessionId  = sm.SessionId  AND sj.StudentId  = sm.StudentId   
  WHERE sm.GroupCode = @GroupCode AND sm.BranchCode = @BranchCode AND sm.StudentId = @StudentId  AND sm.SessionId = @SessionId 
 AND (@SectionId = 0 OR SM.SectionId = @SectionId)
    AND (
            @SelectedClasses IS NULL
            OR @SelectedClasses = ''
            OR SM.ClassCode IN
               (
                   SELECT LTRIM(RTRIM(value))
                   FROM STRING_SPLIT(@SelectedClasses, ',')
               )
        );
        OPEN CUR_STUDENT;
        FETCH NEXT FROM CUR_STUDENT INTO @StudentId;
        WHILE @@FETCH_STATUS = 0
        BEGIN
    if(cast(@admDate as date) >cast(@FeeDueDate as date) ) 
   set @FeeDueDate = @admDate    
   DECLARE CUR_STUDENT CURSOR LOCAL FAST_FORWARD FOR  SELECT StudentId FROM studentmaster WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND ClassCode = @ClassCode AND PeriodType = @PeriodType AND IsValid = 1 AND (StudentId = @StudentId OR @StudentId = 0)AND (SectionId = @SectionId OR @SectionId = 0) OPEN CUR_STUDENT FETCH NEXT FROM CUR_STUDENT INTO @StudentId WHILE @@FETCH_STATUS = 0  
   BEGIN   
    if(@StudentFeeTemplate = 0)    
    begin    
     SELECT  @ErrorCode  = -11                             -- = -11 Fee Template not mapped with student    
     insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)    
     values('Finance','GenerateInvoice','Usp_GenerateStudentInvoiceWithTemplat','Fee Template Not Mapped - '+@StudentId, GETDATE())    
    end    
    else    
    begin    
     if not exists(select * from V3M_FIN_MstInvoice   where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and StudentId = @StudentId and PeriodId = @PeriodId and Ch_Status > 0)    
     begin    
      set @ArrearAmount = 0    
      --=====================================Creating #Table for Result==================================================================    
      create table #tblChallanDetails(FeeHeadId int, HeadName varchar(50), HeadAmount decimal(18,2), ConcessionAmount decimal(18,2), AllMonthsDisplay varchar(100),      
      AllMonths varchar(50), TotalConcession decimal(18,2), TotalAmount decimal(18,2), DisplayOrder int, HeadType varchar(50), FeeType varchar(50), NoMonth INT DEFAULT 0 )       
      --=================================================================================================================================    
      insert into #tblChallanDetails    
	  
	  Exec V3M_FIN_UspGetFeeDetailsForInvoiceWithTemplate @GroupCode,@BranchCode, @SessionId,@ClassCode,@StudentId,@PeriodId   
      --===================================Createing Challan===================================================================    
      DECLARE @InvoiceId bigint,@InvoiceNo varchar(20),@ChallanTotal decimal(18,2),@TotalConcession decimal(18,2)    
      set @InvoiceId=0    
      set @InvoiceNo=0    
      set @ChallanTotal=0    
      set @TotalConcession=0    
      set @ArrearAmount = 0    
      -----------------------------------------------Getting total amount---------------------------------------------------    
      select @ChallanTotal=isnull(sum(TotalAmount),0),@TotalConcession=isnull(sum(TotalConcession),0) from #tblChallanDetails    
        
      --===========================================Calculating Arrier Amount================================================    
      select @ArrearAmount= dbo.Fn_GetStudentBalance(@Groupcode, @BranchCode, @SessionId, @StudentId)     
      Set @ArrearAmount=isnull(@ArrearAmount,0)    
         
      --========================================================================================================================    
      IF NOT EXISTS( select @InvoiceNo from V3M_FIN_MstInvoice  as MC inner join V3M_FIN_Mst_FeeCollectionPeriodConfig as FCP    
      on MC.PeriodId=FCP.PeriodId    
      where MC.Ch_Status>0 and MC.GroupCode=@GroupCode and MC.BranchCode=@BranchCode    
      and MC.SessionId=@SessionId and MC.StudentId=@StudentId and MC.PeriodId>=@PeriodId and FCP.PeriodType=@PeriodType    
      )     
      BEGIN    
    
     EXEC UspGenerateID @GroupCode, @BranchCode,'', @SessionId,'StudentChallan', @InvoiceNo output  
    Select @InvoiceId = ISNULL(MAX(InvoiceId),0) + 1 from V3M_FIN_MstInvoice  
    
       -------------------------------------------------------------------------------------    
       if exists(select * from V3M_FIN_MstInvoice where groupcode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and IsActive=1)    
       BEGIN    
        -----=====================================Select previous actvie challan details=============================================================================    
         
        Update V3M_FIN_MstInvoice set IsActive=0 where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and InvoiceId!=@InvoiceId    
       END    
	   
       insert into V3M_FIN_MstInvoice(InvoiceId,GroupCode,BranchCode,SessionId,StudentId,ClassCode,InvoiceNo,PeriodId,TotalAmount,TotalConcession,    
        ReceivedAmount,ArrearAmount,IsActive,Ch_Status,BankId,AccountNo,CretaedBy,CreatedDate,DueDate,ActualReceived,FeeTemplateId,SocietyId)     
       values(@InvoiceId,@GroupCode,@BranchCode,@SessionId,@StudentId,@ClassCode,@InvoiceNo,@PeriodId,@ChallanTotal,@TotalConcession,    
        0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0,@StudentFeeTemplate,@SocietyId)    
           
       --============================================Recording Student Transcation for creating challan===================================    
       DECLARE @Narration VARCHAR(150),@TransDate datetime,@Result int,@tmpChallanTotal decimal,@PeriodName varchar(50)    
       select @PeriodName=PeriodName from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId    
           
       Set @Narration='Challan for '+@PeriodName+'/dated:'+convert(varchar,getdate(),103);    
       set @TransDate=getdate();    
          
       set @tmpChallanTotal=@ChallanTotal-@TotalConcession     
       exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@SessionId,@StudentId,@InvoiceNo,'CHALLAN',@Narration,@tmpChallanTotal,'','',@TransDate,@CreatedBy,@Result output    
     
       --===================================Creating cursor for generated fee head===============================================    
       DECLARE @CH_FeeHeadId int,@CH_HeadAmount DECIMAL(18,2),@AllMonths INT,@DisplayOrder INT,@ConcessionAmt Decimal(18,2)    
    
       DECLARE CURR_CHALLAN CURSOR FOR    
        select FeeHeadId,HeadAmount,AllMonths,DisplayOrder,TotalConcession from #tblChallanDetails    
       OPEN CURR_CHALLAN    
       FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt    
       WHILE @@FETCH_STATUS=0    
       BEGIN    
        Declare @DetInvoiceId bigint    
        Select @DetInvoiceId=isnull(Max(DetInvoiceId),0)+1 from V3M_FIN_DetInvoice    
        if(@CH_HeadAmount = @ConcessionAmt)    
        begin    
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status,ReceivedDate)    
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,3, getdate())    
        end    
        else    
        begin    
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)    
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,1)    
        end    
        --Rakesh FeeHeadLedger verification    
        insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,    
         Narration,IsValid,InvoiceId,TransDate,TransBy,CRDR,EnteredVia)    
        values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, @CH_HeadAmount,'STUDENT', @StudentID, 'DUE',     
         @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_Usp_GenerateStudentInvoiceWithTemplat')     
        if @ConcessionAmt > 0     
        begin      
         --Rakesh FeeHeadLedger verification    
         insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,    
          Narration,IsValid,InvoiceId,TransDate,TransBy, CRDR,EnteredVia)    
         values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, - @ConcessionAmt,'STUDENT', @StudentID, 'CON',     
          @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy, 'DR','Pro_Usp_GenerateStudentInvoiceWithTemplat')    
        end    
        FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt    
       END    
       CLOSE CURR_CHALLAN     
       DEALLOCATE CURR_CHALLAN    
       --=========================================================================================================================    
       drop table #tblChallanDetails    
      end     
     END    
    end    
    FETCH FROM CUR_STUDENT INTO @StudentID     
   END    
   CLOSE CUR_STUDENT     
   DEALLOCATE CUR_STUDENT    
      Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @SessionId,'StudentChallan'  
   COMMIT TRANSACTION    
   SELECT  @ErrorCode  = 0    
   RETURN @ErrorCode                              -- =0 if success,  <>0 if failure    
 END 
 End
 TRY    
 BEGIN CATCH    
  ROLLBACK TRANSACTION    
  print ERROR_Message()    
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)    
  values('Finance','GenerateInvoice','Usp_GenerateStudentInvoiceWithTemplat',@@ERROR, GETDATE())    
  RETURN @ErrorCode                               -- =0 if success,  <>0 if failure    
 END CATCH    
END     

/*    
-- exec V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat '01','01',1043,'Class1,Class2',18,'Quarterly',1,80050,'V3M',0  
*/    
  
  
  
  
  

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at Usp_GenerateStudentChallanForAllClassWithTemplateDemoForStudent - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat


--ALTER   PROC [dbo].[V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat] @GroupCode varchar(20),@BranchCode varchar(20),
--@SessionId bigint,@ClassCode VARCHAR(50),@SectionId int,@PeriodType varchar(20),
--@PeriodId int ,@StudentId bigint ,@CreatedBy varchar(50),@InvoiceTypeId int,@SocietyId int=0
--AS
    ALTER PROC [dbo].[V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat] @GroupCode varchar(20), @BranchCode varchar(20),
    @SessionId bigint, @ClassCode VARCHAR(50), @SectionId int, @PeriodType varchar(20),
    @PeriodId int, @StudentId bigint, @CreatedBy varchar(50), @InvoiceTypeId int, @SocietyId int = 0,
    @FeeTemplateId int = NULL 
as
 DECLARE @DueToDate date
 DECLARE @ErrorCode  int=0
 DECLARE @BankId int=0
 DECLARE @AccountNO varchar(50)=''
 DECLARE @FeeDueDate datetime,@admDate datetime
 DECLARE @ArrearAmount decimal(18,2)=0
 DECLARE @StudentBalanceAmount decimal(18,2)=0
 Declare  @StudentChalanNo  varchar(50)
 Declare  @AdjArrear varchar(50)='0'
BEGIN
 BEGIN TRY
  BEGIN TRANSACTION
  --===========================================Getting Challan Bank Details============================
  Select @BankId=BA.BankId,@AccountNO=BA.AccountNo
  From V3M_FIN_MstBank as MB
  inner join V3M_FIN_MstBankAccount BA
  on MB.BankId =BA.BankId and MB.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS =BA.GroupCode COLLATE SQL_Latin1_General_CP1_CI_AS
  where BA.IsValid=1 and  BA.GroupCode=@GroupCode and BA.BranchCode=@BranchCode and BA.ForFee=1 and BA.SocietyId=@SocietyId
  if(@SectionId =0)
   set @SectionId = 0;
  select @FeeDueDate=FeeDueDate from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId

--  DECLARE @StudentFeeTemplate INT = 0;
--SELECT TOP 1 @admDate = CAST(AdmissionDate AS DATE),@StudentFeeTemplate = sj.FeeTemplateId  FROM studentmaster sm
--INNER JOIN StudentJourney sj
--    ON sj.GroupCode COLLATE DATABASE_DEFAULT = sm.GroupCode COLLATE DATABASE_DEFAULT
--    AND sj.BranchCode COLLATE DATABASE_DEFAULT = sm.BranchCode COLLATE DATABASE_DEFAULT  AND sj.SessionId  = sm.SessionId  AND sj.StudentId  = sm.StudentId
--WHERE sm.GroupCode = @GroupCode AND sm.BranchCode = @BranchCode AND sm.StudentId = @StudentId  AND sm.SessionId = @SessionId;
--    if(cast(@admDate as date) >cast(@FeeDueDate as date) )
--   set @FeeDueDate = @admDate
    DECLARE @StudentFeeTemplate INT = 0, @JourneyTemplate INT = 0;

    SELECT TOP 1 @admDate = CAST(sm.AdmissionDate AS DATE),
                 @JourneyTemplate = ISNULL(sj.FeeTemplateId, 0)
    FROM studentmaster sm
    LEFT JOIN StudentJourney sj      -- LEFT: journey row na ho tab bhi admission date mile
        ON sj.GroupCode COLLATE DATABASE_DEFAULT = sm.GroupCode COLLATE DATABASE_DEFAULT
       AND sj.BranchCode COLLATE DATABASE_DEFAULT = sm.BranchCode COLLATE DATABASE_DEFAULT
       AND sj.SessionId = sm.SessionId AND sj.StudentId = sm.StudentId
    WHERE sm.GroupCode = @GroupCode AND sm.BranchCode = @BranchCode
      AND sm.StudentId = @StudentId AND sm.SessionId = @SessionId;

    -- Class-wise call: wrapper ne class default template bheja
    -- Student-wise call: StudentJourney waala template
    SET @StudentFeeTemplate = CASE WHEN @FeeTemplateId IS NULL THEN @JourneyTemplate
                                   ELSE @FeeTemplateId END;

    if(cast(@admDate as date) > cast(@FeeDueDate as date))
        set @FeeDueDate = @admDate
   DECLARE CUR_STUDENT CURSOR LOCAL FAST_FORWARD FOR  SELECT StudentId FROM studentmaster WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND ClassCode = @ClassCode AND PeriodType = @PeriodType AND IsValid = 1 AND (StudentId = @StudentId OR @StudentId = 0)AND (SectionId = @SectionId OR @SectionId = 0) OPEN CUR_STUDENT FETCH NEXT FROM CUR_STUDENT INTO @StudentId WHILE @@FETCH_STATUS = 0
   BEGIN
    if(@StudentFeeTemplate = 0)
    begin
     SELECT  @ErrorCode  = -11                             -- = -11 Fee Template not mapped with student
     insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)   
     values('Finance','GenerateInvoice','Usp_GenerateStudentInvoiceWithTemplat','Fee Template Not Mapped - '+@StudentId, GETDATE())
    end
    else
    begin
     if not exists(select * from V3M_FIN_MstInvoice where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId
	 and StudentId = @StudentId and PeriodId = @PeriodId AND InvType = @InvoiceTypeId and Ch_Status > 0 )
     begin
      set @ArrearAmount = 0
      --=====================================Creating #Table for Result==================================================================
      create table #tblChallanDetails(FeeHeadId int, HeadName varchar(50), HeadAmount decimal(18,2), ConcessionAmount decimal(18,2), AllMonthsDisplay varchar(100),
      AllMonths varchar(50), TotalConcession decimal(18,2), TotalAmount decimal(18,2), DisplayOrder int, HeadType varchar(50), FeeType varchar(50), NoMonth INT DEFAULT 0 )
      --=================================================================================================================================
      insert into #tblChallanDetails
	  Exec V3M_FIN_UspGetFeeDetailsForInvoiceWithTemplate @GroupCode,@BranchCode, @SessionId,@ClassCode,@StudentId,@PeriodId,@InvoiceTypeId
      --===================================Createing Challan===================================================================
      DECLARE @InvoiceId bigint,@InvoiceNo varchar(20),@ChallanTotal decimal(18,2),@TotalConcession decimal(18,2)
      set @InvoiceId=0
      set @InvoiceNo=0
      set @ChallanTotal=0
      set @TotalConcession=0
      set @ArrearAmount = 0
      -----------------------------------------------Getting total amount---------------------------------------------------
      select @ChallanTotal=isnull(sum(TotalAmount),0),@TotalConcession=isnull(sum(TotalConcession),0) from #tblChallanDetails
      --===========================================Calculating Arrier Amount================================================
      select @ArrearAmount= dbo.Fn_GetStudentBalance(@Groupcode, @BranchCode, @SessionId, @StudentId)
      Set @ArrearAmount=isnull(@ArrearAmount,0)
      --========================================================================================================================
      IF NOT EXISTS( select @InvoiceNo from V3M_FIN_MstInvoice  as MC inner join V3M_FIN_Mst_FeeCollectionPeriodConfig as FCP
      on MC.PeriodId=FCP.PeriodId
      where MC.Ch_Status>0 and MC.GroupCode=@GroupCode and MC.BranchCode=@BranchCode
      and MC.SessionId=@SessionId and MC.StudentId=@StudentId and MC.PeriodId>=@PeriodId and FCP.PeriodType=@PeriodType   AND MC.InvType = @InvoiceTypeId
      )
      BEGIN
       ---------------------Getting challan no and ID------------------------------
     EXEC UspGenerateID @GroupCode, @BranchCode,'', @SessionId,'StudentChallan', @InvoiceNo output
     -- FIX (script 003): concurrency-safe replacement for "SELECT MAX(InvoiceId)+1"
     SELECT @InvoiceId = NEXT VALUE FOR dbo.Seq_V3M_FIN_InvoiceId;
       -------------------------------------------------------------------------------------
       if exists(select * from V3M_FIN_MstInvoice where groupcode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and IsActive=1)
       BEGIN
        Update V3M_FIN_MstInvoice set IsActive=0 where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and InvoiceId!=@InvoiceId
       END

       insert into V3M_FIN_MstInvoice(InvoiceId,GroupCode,BranchCode,SessionId,StudentId,ClassCode,InvoiceNo,PeriodId,TotalAmount,TotalConcession,
        ReceivedAmount,ArrearAmount,IsActive,Ch_Status,BankId,AccountNo,CretaedBy,CreatedDate,DueDate,ActualReceived,FeeTemplateId,SocietyId,InvType)
       values(@InvoiceId,@GroupCode,@BranchCode,@SessionId,@StudentId,@ClassCode,@InvoiceNo,@PeriodId,@ChallanTotal,@TotalConcession,
        0,@ArrearAmount,1,1,@BankId,@AccountNO,@CreatedBy,getdate(),@FeeDueDate,0,@StudentFeeTemplate,@SocietyId,@InvoiceTypeId)

       --============================================Recording Student Transcation for creating challan===================================
       DECLARE @Narration VARCHAR(150),@TransDate datetime,@Result int,@tmpChallanTotal decimal,@PeriodName varchar(50)
       --select @PeriodName=PeriodName from V3M_FIN_Mst_FeeCollectionPeriodConfig where PeriodId=@PeriodId
       SELECT @PeriodName = PeriodName + CAST(YEAR(GETDATE()) AS VARCHAR(4)) FROM V3M_FIN_Mst_FeeCollectionPeriodConfig WHERE PeriodId = @PeriodId;
       Set @Narration='Challan for '+@PeriodName+'/dated:'+convert(varchar,getdate(),103);
       set @TransDate=getdate();

       set @tmpChallanTotal=@ChallanTotal-@TotalConcession
       exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@SessionId,@StudentId,@InvoiceNo,'CHALLAN',@Narration,@tmpChallanTotal,'','',@TransDate,@CreatedBy,@Result output

       --===================================Creating cursor for generated fee head===============================================
       DECLARE @CH_FeeHeadId int,@CH_HeadAmount DECIMAL(18,2),@AllMonths INT,@DisplayOrder INT,@ConcessionAmt Decimal(18,2)

       DECLARE CURR_CHALLAN CURSOR FOR
        select FeeHeadId,HeadAmount,AllMonths,DisplayOrder,TotalConcession from #tblChallanDetails
       OPEN CURR_CHALLAN
       FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt
       WHILE @@FETCH_STATUS=0
       BEGIN
        Declare @DetInvoiceId bigint
        -- FIX (script 003): concurrency-safe replacement for "SELECT MAX(DetInvoiceId)+1"
        SELECT @DetInvoiceId = NEXT VALUE FOR dbo.Seq_V3M_FIN_DetInvoiceId;
        if(@CH_HeadAmount = @ConcessionAmt)
        begin
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status,ReceivedDate)
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,3, getdate())
        end
        else
        begin
         insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)
         values(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0,0,0,1,1)
        end
        insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,
         Narration,IsValid,InvoiceId,TransDate,TransBy,CRDR,EnteredVia,SessionId,CreatedDate)
        values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, @CH_HeadAmount,'STUDENT', @StudentID, 'DUE',
         @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_Usp_GenerateStudentInvoiceWithTemplat',@SessionId,GETDATE())
        if @ConcessionAmt > 0
        begin
         insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,
          Narration,IsValid,InvoiceId,TransDate,TransBy, CRDR,EnteredVia,SessionId,CreatedDate)
         values(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(getdate()),@SessionId, - @ConcessionAmt,'STUDENT', @StudentID, 'CON',
          @PeriodName, 1, @InvoiceId, GETDATE(),@CreatedBy, 'DR','Pro_Usp_GenerateStudentInvoiceWithTemplat',@SessionId,GETDATE())
        end
        FETCH NEXT FROM CURR_CHALLAN INTO @CH_FeeHeadId,@CH_HeadAmount,@AllMonths,@DisplayOrder,@ConcessionAmt
       END
       CLOSE CURR_CHALLAN
       DEALLOCATE CURR_CHALLAN
       --=========================================================================================================================
       drop table #tblChallanDetails
      end
     END
    end
    FETCH FROM CUR_STUDENT INTO @StudentID
   END
   CLOSE CUR_STUDENT
   DEALLOCATE CUR_STUDENT
      Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @SessionId,'StudentChallan'
   COMMIT TRANSACTION
   SELECT  @ErrorCode  = 0
   RETURN @ErrorCode                              -- =0 if success,  <>0 if failure
 END TRY
 BEGIN CATCH
  ROLLBACK TRANSACTION
  print ERROR_Message()
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
  values('Finance','GenerateInvoice','Usp_GenerateStudentInvoiceWithTemplat',@@ERROR, GETDATE())
  RETURN @ErrorCode                               -- =0 if success,  <>0 if failure
 END CATCH

END

--- Select * Det_ExceptionDetails order by 1 desc

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_GeneratedStudentReceiptNo
ALTER PROCEDURE [dbo].[V3M_FIN_GeneratedStudentReceiptNo] @StudentId BIGINT, @GroupCode VARCHAR(10),@BranchCode VARCHAR(10),
 @CurrentSession BIGINT,@AdmissionSession BIGINT, @ClassCode VARCHAR(20), @TotalAmount DECIMAL(18,2), @PaidAmount DECIMAL(18,2),
 @Concession DECIMAL(18,2), @Balance DECIMAL(18,2), @PaymentMode VARCHAR(10), @ChequeNo VARCHAR(50), @ChequeDate date,
 @ChequeBank VARCHAR(80), @LateFee DECIMAL(18,2), @PreviousBalance DECIMAL(18,2), @Remark VARCHAR(250), @IsAdmissionReceipt int,
 @PeriodType varchar(20), @PeriodId INT, @SocietyId int, @ChequeType varchar(50), @CreatedBy VARCHAR(20), @StudentReceiptNo varchar(50) output,
 @ReceiptId bigint output, @ReceiptDate date
As
SET NOCOUNT ON
SET XACT_ABORT ON
declare @Narration nvarchar(150)=''
declare @TransDate datetime
declare @Result int = 0
declare @InvoiceId bigint=0,@BankId int=0, @AccountNo varchar(50)='', @InvoiceNo varchar(50)='0' ,@TemplateId int
BEGIN
 set @ReceiptId = 0
 BEGIN TRY
  BEGIN TRAN

  --------------------------Generating Student Receipt No----------------------------------
  --EXEC V3M_Fin_StudentReceiptNoPattern @GroupCode,@BranchCode,@AdmissionSession,@StudentReceiptNo output

  EXEC UspGenerateID @GroupCode, @BranchCode,'', @AdmissionSession,'StudentReceipt', @StudentReceiptNo output
  Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @AdmissionSession,'StudentReceipt',@SocietyId

  IF @StudentReceiptNo IS NULL OR @StudentReceiptNo=''
  BEGIN
   -- Nothing generated: undo the key counter increment as well.
   IF @@TRANCOUNT > 0
    ROLLBACK TRAN
   set @StudentReceiptNo = NULL
   RETURN
  END

  -- ===========================================Getting Challan Bank Details==============================================================
  Select @TemplateId= DefaultFeeTemplateId From V3M_FIN_ClassDefaultFeeTemplate  where GroupCode=@GroupCode And BranchCode=@BranchCode And SessionId =@CurrentSession And ClassCode=@ClassCode
	SELECT @BankId = BA.BankId,@AccountNO = BA.AccountNo
	FROM dbo.V3M_FIN_MstBank AS MB
	INNER JOIN dbo.V3M_FIN_MstBankAccount AS BA
		ON MB.BankId = BA.BankId AND MB.GroupCode COLLATE DATABASE_DEFAULT= BA.GroupCode COLLATE DATABASE_DEFAULT
	WHERE BA.IsValid = 1
	  AND BA.GroupCode COLLATE DATABASE_DEFAULT = @GroupCode COLLATE DATABASE_DEFAULT
	  AND BA.BranchCode COLLATE DATABASE_DEFAULT= @BranchCode COLLATE DATABASE_DEFAULT
	  AND BA.SocietyId = @SocietyId AND BA.ForFee = 1;
  --------------------------Generating Student Challan No----------------------------------
  --exec V3M_Fin_StudentInvoiceNoPattern @GroupCode,@BranchCode,@AdmissionSession,@InvoiceNo output
    EXEC UspGenerateID @GroupCode, @BranchCode,'', @AdmissionSession,'StudentChallan', @InvoiceNo output
    Exec Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @AdmissionSession,'StudentChallan',''
    select @InvoiceId=isnull(Max(InvoiceId),0)+1 from V3M_FIN_MstInvoice WITH (UPDLOCK, HOLDLOCK)

  declare @DueDate date
  select @DueDate = FeeDueDate from V3M_FIN_Mst_FeeCollectionPeriodConfig where GroupCode = @GroupCode and BranchCode=@BranchCode and PeriodId = @PeriodId
  if(@DueDate is null)
	set @DueDate = Getdate()
  insert into V3M_FIN_MstInvoice(InvoiceId,GroupCode,BranchCode,SessionId,StudentId,ClassCode,InvoiceNo,PeriodId,TotalAmount,TotalConcession,ReceivedAmount,
   ArrearAmount,IsActive,Ch_Status,BankId,AccountNo,CretaedBy,CreatedDate,DueDate,ActualReceived,SocietyId,FeeTemplateId)
  values(@InvoiceId,@GroupCode,@BranchCode,@AdmissionSession,@StudentId,@ClassCode,@InvoiceNo,@PeriodId,@TotalAmount,@Concession,0,0,1,1,@BankId,@AccountNO,
  @CreatedBy,getdate(),@DueDate,0,@SocietyId,@TemplateId)
  --============================================Concession Entry into Fee Head Ledger=======================================
  --insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,
  --RefType,RefId,TransType,Narration,IsValid,InvoiceId,TransDate,TransBy)
  --values(@GroupCode, @BranchCode, @AdmissionSession, 0, 0, MONTH(getdate()),@AdmissionSession, -@Concession,'STUDENT', @StudentId, 'CON',
  --'Concession given in challan  ', 1, @InvoiceId, GETDATE(),@CreatedBy)

  --============================================Recording Student Transcation================================================

  DECLARE @ActualChallanAmount decimal

  Set @Narration='Admision challan-'+isnull(@InvoiceNo,'')+' /dated:'+Convert(varchar,getdate(),103)

  set @TransDate=getdate()
  set @ActualChallanAmount = @TotalAmount - @Concession

  exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@AdmissionSession,@StudentId,@InvoiceNo,'Challan',@Narration,@ActualChallanAmount,@PaymentMode,@ChequeNo,@TransDate,@CreatedBy,@Result output

  -- ====================================================== Student Receipt =======================================================================
  INSERT INTO V3M_FIN_StudentReceiptMaster(GroupCode,BranchCode,SessionId,ClassCode, SectionId, StudentId,ReceiptNo,PreviousBalance,
    TotalAmount,PaidAmount,Concession, Balance,LateFee,Advance,AdjustedFromAdvance,PaymentMode,ChequeNo,ChequeDate,ChequeBank, ChequeType,
    Valid,CreatedBy,CreatedDate,IsAdmissionReceipt,Remark,SocietyId, GeneratedDate, IsActive, InvoiceId)
  VALUES(@GroupCode,@BranchCode,@AdmissionSession,@ClassCode, 0,@StudentId,@StudentReceiptNo,@PreviousBalance,
    @TotalAmount,@PaidAmount,@Concession,@Balance,@LateFee,0,0,@PaymentMode,@ChequeNo, @ChequeDate, @ChequeBank, @ChequeType,
    1,@CreatedBy,@ReceiptDate,@IsAdmissionReceipt,@Remark,@SocietyId, Getdate(), 1, @InvoiceId)

  -- SCOPE_IDENTITY: @@IDENTITY can return an id from a trigger or nested-proc insert.
  select @ReceiptId = SCOPE_IDENTITY()
  --============================================Recording Student Transcation================================================

  if not exists (select * from V3M_Mst_Society where GroupCode = @GroupCode and BranchCode = @BranchCode and SocietyId = @SocietyId and BankProcessApl = 1 and IsValid = 1)
  begin
   Set @Narration='Admision Receipt No: '+@StudentReceiptNo +'/dated:'+Convert(varchar,getdate(),103)
   set @TransDate=getdate()
   DECLARE @tmpPaidAmount decimal

   set @tmpPaidAmount=0-@PaidAmount

   exec V3M_FIN_StudentTrans @GroupCode,@BranchCode,@AdmissionSession,@StudentId,@StudentReceiptNo,'Student Receipt',@Narration,@tmpPaidAmount,@PaymentMode,@ChequeNo,@TransDate,@CreatedBy,@Result output
  end
  --============================================Update Student Master=========================================================

  -- Was StudentNo=@StudentId: StudentNo is varchar, so every StudentNo was converted to bigint
  -- ("Error converting data type varchar to bigint") and the wrong column was matched.
  Update StudentMaster set PeriodType=@PeriodType where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId

  --------------------------------------------------Balance can not be less than 0 from front end ----------------------------------------------------------------------------
  --if(@Balance<0)
  --begin
  --IF EXISTS(SELECT * FROM v3m_fin_Studentbalance WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND StudentId=@StudentId AND SessionId=@AdmissionSession)
  -- UPDATE v3m_fin_Studentbalance set Balance= @Balance WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND StudentId=@StudentId  AND SessionId=@AdmissionSession
  --ELSE
  -- INSERT INTO v3m_fin_Studentbalance(GroupCode,BranchCode,StudentId,Balance,CreatedDate,CreatedBy,SessionId)
  -- VALUES(@GroupCode,@BranchCode, @StudentId,@Balance,GETDATE(),@CreatedBy,@AdmissionSession)

  --if @Balance < 0
  -- insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,TransDate,Narration,IsValid,InvoiceId,TransBy)
  -- values(@GroupCode, @BranchCode, @CurrentSession, 0, 0,MONTH(Getdate()),@AdmissionSession,@Balance ,'STUDENT', @StudentId, 'SB', GETDATE(),'Admission Receipt', 1,0,@CreatedBy)
  --end

  --SKG Create a Journal Entry For Receipt from Student
	declare @NeedToManageAc varchar(5), @ControlNo varchar(30)
	if exists(Select isnull(KeyValue,'0') From DefaultSettings where GroupCode = @GroupCode and BranchCode = @BranchCode and KeyName = 'NeedToManageAc')
	begin

		Select @NeedToManageAc = trim(isnull(KeyValue,'0')) From DefaultSettings Where GroupCode = @GroupCode and BranchCode = @BranchCode and KeyName = 'NeedToManageAc'
		if(@NeedToManageAc = '1')
		Begin
			Select top 1 @ControlNo = ControlNo from StudentMaster where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @CurrentSession and StudentId = @StudentId
			declare @tmpNarration varchar(500) = isnull(@Narration,'') + ' by Student No : ' + isnull(@ControlNo,'')

			Exec V3M_Fin_ReceiptJournalEntry @GroupCode, @BranchCode,@SocietyId, @CurrentSession, @ReceiptDate, @PaymentMode, @PaidAmount, @tmpNarration, @CreatedBy, 0
		End
	end
	--SKG Create a Journal Entry For Receipt from Student

  COMMIT TRAN
 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT > 0
   ROLLBACK TRAN

  set @StudentReceiptNo = NULL
  set @ReceiptId = 0

  INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate, BranchCode, [User])
  VALUES('Procedure', 'FinanceMNGT.AdmitChild', '[V3M_FIN_GeneratedStudentReceiptNo]',
   CONCAT(ERROR_MESSAGE(), ' Line ', ERROR_LINE()), GETDATE(), @BranchCode, @CreatedBy);

  THROW;
 END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_GeneratedStudentReceiptNo - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_GetInvoiceMasterDetails
-- Exec V3M_FIN_GetInvoiceMasterDetails '01','01','20',1,1,'EditInvoice'
 ALTER PROCEDURE [dbo].[V3M_FIN_GetInvoiceMasterDetails]
 (
    @GroupCode     VARCHAR(10),
    @BranchCode    VARCHAR(10),
    @InvoiceId     VARCHAR(MAX),  
    @IsForBalPrint BIT = 0,
	@InvType       INT =0,
    @ValueType    VARCHAR(10)=null
 )
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @InvoiceIds dbo.TVP_InvoiceIds;
	DECLARE @NewInvoiceId BIGINT;
	--if(@InvType=0)	
	--BEGIN
	--	INSERT INTO @InvoiceIds (InvoiceId)
	--	SELECT TRY_CAST(value AS BIGINT)
	--	FROM STRING_SPLIT(@InvoiceId, ',')
	--	WHERE TRY_CAST(value AS BIGINT) IS NOT NULL;
	--END
	--ELSE
	--BEGIN
	--	SELECT TOP 1 @NewInvoiceId = m2.InvoiceId FROM V3M_FIN_MstInvoice m1
	--   INNER JOIN V3M_FIN_MstInvoice m2 ON m1.StudentId = m2.StudentId AND m1.GroupCode = m2.GroupCode AND m1.BranchCode = m2.BranchCode 
	--   AND m1.SessionId = m2.SessionId WHERE m1.InvoiceId = TRY_CAST(@InvoiceId AS BIGINT)
 --     AND m2.InvType = @InvType ORDER BY m2.InvoiceId DESC;
	--	IF @NewInvoiceId IS NOT NULL
	--	BEGIN
	--		INSERT INTO @InvoiceIds(InvoiceId)
	--		VALUES(@NewInvoiceId);
	--	END
	--END
	IF (@InvType = 0)
	BEGIN
		INSERT INTO @InvoiceIds (InvoiceId)
		SELECT TRY_CAST(value AS BIGINT)
		FROM STRING_SPLIT(@InvoiceId, ',')
		WHERE TRY_CAST(value AS BIGINT) IS NOT NULL;
	END
	ELSE
	BEGIN
		SET @NewInvoiceId = TRY_CAST(@InvoiceId AS BIGINT);

		IF @NewInvoiceId IS NOT NULL
		BEGIN
			INSERT INTO @InvoiceIds (InvoiceId)
			VALUES (@NewInvoiceId);
		END
	END
    SELECT vfmi.InvoiceId, vfmi.InvoiceNo, vfmi.PeriodId, vfmi.SessionId,vsd.SessionName,vsd.RouteDistance, ISNULL(fcpc.PeriodName, 'Miscellaneous') AS PeriodName,
	vfmi.ClassCode, vsd.ClassName, vsd.ClassSection, vfmi.StudentId, vsd.ControlNo, vsd.StudentName, vsd.FatherName,
	vsd.MotherName, vsd.DistanceName, vsd.ISTRANSPORTREQUIRED, dbo.fn_getStudentRouteName( vfmi.GroupCode, vfmi.BranchCode, 
	vfmi.StudentId, '0') AS RouteName, vsd.NextToKin, vsd.NextToKinName, vsd.SMSMobileNo, vfmi.AccountNo, vfmi.DueDate,
	ROUND(vfmi.TotalAmount + vfmi.ArrearAmount, 0)  AS TotalAmount, vfmi.TotalConcession, vfmi.ArrearAmount,vfmi.ReceivedAmount, 
    vsd.SocietyId, ISNULL(fcpc.PeriodType,'Other')  AS PeriodType,vfmi.LateCharges,
    vfmi.CreatedDate, mc.ConcessionId, mc.Concession,ISNULL(fcpc.QuarterName, '') AS QuarterName,vfmi.SessionId,
	dbo.Fn_GetStudentBalance( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId) AS StudentBalance,
	dbo.Fun_V3M_FIN_CalculateStudentBalanceTillDate( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId)AS PreviousBalance, 
	dbo.Fun_V3M_FIN_CurrentInvoicePreBalance( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, vfmi.InvoiceId) AS InvoicePreBalance, 
	dbo.V3M_FIN_CalculateLateFee( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, CONVERT(VARCHAR(10), 
	GETDATE(), 120), fcpc.dueto) AS StudentLateFee, ISNULL(( SELECT ISNULL(sb.Balance, 0)FROM V3M_Fin_StudentBalance sb
	WHERE sb.StudentId  = vsd.StudentId AND sb.GroupCode  = vsd.GroupCode AND sb.BranchCode = vsd.BranchCode
	AND sb.SessionId  = vsd.SessionId AND ISNULL(sb.IsValid, 1) = 1 ), 0) AS PreviousAdvance,( SELECT ISNULL(SUM(ISNULL(TotalAmount, 0)), 0)
	FROM V3M_FIN_AdvanceDonationWaiveOff WHERE GroupCode  = vsd.GroupCode AND BranchCode = vsd.BranchCode
	AND SessionId  = vsd.SessionId AND StudentId  = vsd.StudentId AND Status = 2 AND TransType  = 'Adjustment' )
	* -1   AS AdjustmentApproved, ( SELECT ISNULL(STRING_AGG(CAST(PeriodId AS VARCHAR), ','), '') FROM V3M_FIN_MstInvoice 
	WHERE GroupCode  = vsd.GroupCode AND BranchCode = vsd.BranchCode AND SessionId  = vsd.SessionId AND StudentId  = vsd.StudentId 
	AND ch_status  > 0 AND TotalAmount - ( ISNULL(TotalConcession, 0) + ISNULL(ReceivedAmount,  0) + ISNULL(TotalWaiveOff,   0) ) > 0 )AS PeriodList 
	FROM vwStudentDetails vsd INNER JOIN V3M_FIN_MstInvoice vfmi ON  vsd.GroupCode  = vfmi.GroupCode AND vsd.BranchCode = vfmi.BranchCode 
	AND vsd.SessionId  = vfmi.SessionId AND vsd.StudentId  = vfmi.StudentId INNER JOIN @InvoiceIds ids ON vfmi.InvoiceId = ids.InvoiceId
	LEFT JOIN V3M_FIN_Mst_FeeCollectionPeriodConfig fcpc ON  fcpc.GroupCode  = vfmi.GroupCode AND fcpc.BranchCode = vfmi.BranchCode 
	AND fcpc.SessionId  = vfmi.SessionId AND fcpc.PeriodId   = vfmi.PeriodId LEFT JOIN V3M_FIN_DetConcessionMapToStudent 
	cms ON  cms.GroupCode  COLLATE DATABASE_DEFAULT = vfmi.GroupCode  COLLATE DATABASE_DEFAULT AND cms.BranchCode COLLATE DATABASE_DEFAULT = vfmi.BranchCode
	COLLATE DATABASE_DEFAULT AND cms.SessionId  = vfmi.SessionId AND cms.StudentId  = vfmi.StudentId AND cms.Status= '1' AND 
	vfmi.TotalConcession > 0 LEFT JOIN V3M_FIN_MstConcession mc ON  cms.GroupCode  COLLATE DATABASE_DEFAULT = mc.GroupCode  
	COLLATE DATABASE_DEFAULT AND cms.BranchCode COLLATE DATABASE_DEFAULT = mc.BranchCode COLLATE DATABASE_DEFAULT 
	AND cms.ConcessionId = mc.ConcessionId ORDER BY vfmi.InvoiceId;
    IF EXISTS ( SELECT 1 FROM V3M_FIN_MstInvoice vfmi INNER JOIN @InvoiceIds ids ON vfmi.InvoiceId = ids.InvoiceId
        WHERE vfmi.GroupCode  = @GroupCode AND vfmi.BranchCode = @BranchCode  AND (@InvType = 0 OR vfmi.InvType = @InvType))
    BEGIN
   IF (ISNULL(@ValueType, '') = '')
    BEGIN
        EXEC USP_GetInvoiceFeeHeadDetails @GroupCode, @BranchCode, @InvoiceIds, @IsForBalPrint;  
     END
    ELSE
     BEGIN
        EXEC USP_GetAndEditInvoiceFeeHeadDetail @GroupCode, @BranchCode, @InvoiceIds, @IsForBalPrint;  
       END
    END

END



--Select * from V3M_FIN_MstInvoice where StudentId=80050

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_GetInvoiceMasterDetails - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_AdjustStudentFeeHeadWise_Fast

ALTER PROCEDURE dbo.V3M_FIN_AdjustStudentFeeHeadWise_Fast
    @StudentId          BIGINT,
    @GroupCode          VARCHAR(10),
    @BranchCode         VARCHAR(10),
    @SessionId           BIGINT,
    @ClassCode          VARCHAR(20),
    @SectionId          INT,
    @PaidAmount         DECIMAL(18,2),
    @Balance            DECIMAL(18,2),
    @PaymentMode        VARCHAR(20),
    @ChequeNo           VARCHAR(50)  = '',
    @ChequeDate         DATE         = NULL,
    @ChequeBank         VARCHAR(80)  = '',
    @ChequeType         VARCHAR(20)  = '',
    @LateFee            DECIMAL(18,2) = 0,
    @PreviousBalance    DECIMAL(18,2) = 0,
    @Remark             VARCHAR(250) = '',
    @SocietyId          INT          = 0,
    @CreatedBy          VARCHAR(50),
    @ReceiptDate        DATE,
    @BankAccountId      INT,
    @BankBranch         VARCHAR(100) = '',
    @FeeHeadDetails     dbo.FeeHeadDetailType READONLY,
    @StudentReceiptNo   VARCHAR(50) OUTPUT,
    @ReceiptId          BIGINT OUTPUT
AS
BEGIN
    ------------------------------ Affected Tables -------------------------------------
    -- V3M_FIN_StudentReceiptMaster, V3M_FIN_StudentReceiptDetail
    -- V3M_FIN_MstInvoice, V3M_FIN_DetInvoice
    -- V3M_FIN_StudentLedger, V3M_FIN_FeeHeadLedger, V3M_Fin_StudentBalance
    -- Det_TransactionLog (header-level audit, via Usp_SaveTransactionLog)
    -------------------------------------------------------------------------------------
    SET NOCOUNT ON;
    DECLARE @ErrorCode INT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        ----------------------------------------------------------------------
        -- Receipt number (existing concurrency-safe generator - TABLOCKX)
        ----------------------------------------------------------------------
        EXEC dbo.UspGenerateID @GroupCode, @BranchCode, '', @SessionId, 'StudentReceipt', @StudentReceiptNo OUTPUT;
        EXEC dbo.Usp_UpdateStudentKeyValues @GroupCode, @BranchCode, @SessionId, 'StudentReceipt', @SocietyId;

        IF @StudentReceiptNo IS NULL OR @StudentReceiptNo = ''
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN -1;
        END

        DECLARE @TotalConcession DECIMAL(18,2) = ISNULL((SELECT SUM(Concession) FROM @FeeHeadDetails), 0);
        DECLARE @TransDate DATETIME = @ReceiptDate;

        ----------------------------------------------------------------------
        -- Receipt master header
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_StudentReceiptMaster
            (GroupCode, BranchCode, SessionId, ClassCode, SectionId, StudentId, ReceiptNo, PreviousBalance, TotalAmount,
             PaidAmount, Concession, Balance, LateFee, Advance, AdjustedFromAdvance, PaymentMode, ChequeNo, ChequeDate, ChequeBank, ChequeType, Valid,
             CreatedBy, CreatedDate, IsAdmissionReceipt, Remark, SocietyId, InvoiceId, BankAccountId, GeneratedDate, IsActive, ChequeBankBranch, IsEdited)
        VALUES
            (@GroupCode, @BranchCode, @SessionId, @ClassCode, @SectionId, @StudentId, @StudentReceiptNo, @PreviousBalance, @PaidAmount + @TotalConcession,
             @PaidAmount, @TotalConcession, @Balance, @LateFee, 0, 0, @PaymentMode, @ChequeNo, @ChequeDate, @ChequeBank, @ChequeType, 1,
             @CreatedBy, @ReceiptDate, 0, @Remark, @SocietyId, 0, @BankAccountId, GETDATE(), 1, @BankBranch, 0);

        SET @ReceiptId = SCOPE_IDENTITY();

        ----------------------------------------------------------------------
        -- Receipt detail lines (one per fee-head/due line submitted)
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_StudentReceiptDetail
            (GroupCode, BranchCode, ReceiptNo, ReceiptId, InvoiceId, FeeHeadId, MonthNo, FeeHeadAmount, FeeHeadConcession, PaidAmount, Valid, CreatedBy, CreatedDate, IsAdjusted, AdjustmentAmt)
        SELECT
            @GroupCode, @BranchCode, @StudentReceiptNo, @ReceiptId, di.InvoiceId, f.FeeHeadId, TRY_CAST(f.MonthNoCsv AS INT), f.FeeHeadAmount, f.Concession, f.PaidAmount, 1, @CreatedBy, GETDATE(),
            CASE WHEN ISNULL(f.AdjustAmount, 0) > 0 AND f.DetInvoiceId > 0 THEN 1 ELSE 0 END,
            CASE WHEN f.DetInvoiceId > 0 THEN f.AdjustAmount ELSE NULL END
        FROM @FeeHeadDetails f
        LEFT JOIN dbo.V3M_FIN_DetInvoice di ON di.DetInvoiceId = f.DetInvoiceId AND f.DetInvoiceId > 0
        WHERE f.PaidAmount <> 0 OR f.DetInvoiceId > 0;

        ----------------------------------------------------------------------
        -- Update V3M_FIN_DetInvoice per due line actually paid
        -- Status: 1=due (unchanged), 2=partially received, 3=fully covered
        -- (convention taken from V3M_FIN_Usp_GenerateStudentInvoiceWithTemplat)
        ----------------------------------------------------------------------
        UPDATE di
        SET di.ReceivedAmt   = ISNULL(di.ReceivedAmt, 0) + f.PaidAmount,
            di.WaiveOff      = ISNULL(di.WaiveOff, 0) + ISNULL(f.Deduction, 0),
            di.ReceivedDate  = @ReceiptDate,
            di.UpdatedBy     = @CreatedBy,
            di.UpdatedDate   = GETDATE(),
            di.Status = CASE
                WHEN (ISNULL(di.ReceivedAmt, 0) + f.PaidAmount + ISNULL(di.Concession, 0) + ISNULL(di.WaiveOff, 0) + ISNULL(f.Deduction, 0)) >= di.Amount THEN 3
                ELSE 2
            END
        FROM dbo.V3M_FIN_DetInvoice di
        INNER JOIN @FeeHeadDetails f ON f.DetInvoiceId = di.DetInvoiceId
        WHERE f.DetInvoiceId > 0;

        ----------------------------------------------------------------------
        -- Roll up into V3M_FIN_MstInvoice
        -- Ch_Status convention copied from FIN_Usp_AddFeeHeadToStudentChallan
        ----------------------------------------------------------------------
        ;WITH InvoiceTotals AS (
            SELECT di.InvoiceId, SUM(f.PaidAmount) AS PaidSum
            FROM @FeeHeadDetails f
            INNER JOIN dbo.V3M_FIN_DetInvoice di ON di.DetInvoiceId = f.DetInvoiceId
            WHERE f.DetInvoiceId > 0
            GROUP BY di.InvoiceId
        )
        UPDATE mi
        SET mi.ReceivedAmount = ISNULL(mi.ReceivedAmount, 0) + it.PaidSum,
            mi.ActualReceived = ISNULL(mi.ActualReceived, 0) + it.PaidSum,
            mi.Ch_Status = CASE WHEN ISNULL(mi.ReceivedAmount, 0) + it.PaidSum > 0 THEN 2 ELSE 1 END
        FROM dbo.V3M_FIN_MstInvoice mi
        INNER JOIN InvoiceTotals it ON it.InvoiceId = mi.InvoiceId;

        ----------------------------------------------------------------------
        -- FeeHeadLedger: one RECV (credit) entry per due line actually paid
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            @GroupCode, @BranchCode, @SessionId, f.FeeHeadId, TRY_CAST(f.MonthNoCsv AS INT), MONTH(GETDATE()), @SessionId, -f.PaidAmount, 'STUDENT', @StudentId, 'RECV',
            'Fee received against Receipt No - ' + @StudentReceiptNo, 1, di.InvoiceId, @TransDate, @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast', @SessionId
        FROM @FeeHeadDetails f
        INNER JOIN dbo.V3M_FIN_DetInvoice di ON di.DetInvoiceId = f.DetInvoiceId
        WHERE f.DetInvoiceId > 0 AND f.PaidAmount <> 0;

        -- Waive-off portion, if any, mirrored the same way as FIN_Usp_RemoveFeeHeadFromChallan's 'WO' entries
        INSERT INTO dbo.V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            @GroupCode, @BranchCode, @SessionId, f.FeeHeadId, TRY_CAST(f.MonthNoCsv AS INT), MONTH(GETDATE()), @SessionId, -f.Deduction, 'STUDENT', @StudentId, 'WO',
            'Waive off against Receipt No - ' + @StudentReceiptNo, 1, di.InvoiceId, @TransDate, @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast', @SessionId
        FROM @FeeHeadDetails f
        INNER JOIN dbo.V3M_FIN_DetInvoice di ON di.DetInvoiceId = f.DetInvoiceId
        WHERE f.DetInvoiceId > 0 AND ISNULL(f.Deduction, 0) <> 0;

        ----------------------------------------------------------------------
        -- Advance lines: TVP rows with no DetInvoiceId (not tied to an existing due)
        -- Posted as a credit to the student's own ledger only (mirrors the
        -- 'Pre balance adjusted adv in receipt' SB entry pattern used by the
        -- real V3M_FIN_GeneratedStudentCounterAdjustemntReceipt procedure).
        ----------------------------------------------------------------------
        IF EXISTS (SELECT 1 FROM @FeeHeadDetails WHERE ISNULL(DetInvoiceId, 0) = 0 AND PaidAmount <> 0)
        BEGIN
            INSERT INTO dbo.V3M_FIN_FeeHeadLedger
                (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
            SELECT
                @GroupCode, @BranchCode, @SessionId, f.FeeHeadId, TRY_CAST(f.MonthNoCsv AS INT), MONTH(GETDATE()), @SessionId, -f.PaidAmount, 'STUDENT', @StudentId, 'ADV',
                'Advance received against Receipt No - ' + @StudentReceiptNo, 1, @TransDate, @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast', @SessionId
            FROM @FeeHeadDetails f
            WHERE ISNULL(f.DetInvoiceId, 0) = 0 AND f.PaidAmount <> 0;
        END

        ----------------------------------------------------------------------
        -- Late fee (verbatim pattern from V3M_FIN_GeneratedStudentCounterAdjustemntReceipt)
        ----------------------------------------------------------------------
        IF @LateFee > 0
        BEGIN
            DECLARE @LfResult INT = 0;
            EXEC dbo.V3M_FIN_StudentTrans @GroupCode, @BranchCode, @SessionId, @StudentId, @StudentReceiptNo, 'Late Fee', 'Late Fee Fine', @LateFee, '', '', @TransDate, @CreatedBy, @LfResult OUTPUT;

            INSERT INTO dbo.V3M_FIN_FeeHeadLedger
                (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia)
            VALUES
                (@GroupCode, @BranchCode, @SessionId, 0, MONTH(@ReceiptDate), MONTH(GETDATE()), @SessionId, @LateFee, 'STUDENT', @StudentId, 'LF',
                 'Late Fee for Receipt No - ' + @StudentReceiptNo, 1, @ReceiptId, @ReceiptDate, @CreatedBy, @ReceiptId, 0, 'DR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast');

            INSERT INTO dbo.V3M_FIN_FeeHeadLedger
                (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia)
            VALUES
                (@GroupCode, @BranchCode, @SessionId, 0, MONTH(@ReceiptDate), MONTH(GETDATE()), @SessionId, -@LateFee, 'STUDENT', @StudentId, 'LF',
                 'Late Fee for Receipt No - ' + @StudentReceiptNo, 1, @ReceiptId, @ReceiptDate, @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast');
        END

        ----------------------------------------------------------------------
        -- Student-level running ledger entry for the whole receipt
        ----------------------------------------------------------------------
        DECLARE @Narration VARCHAR(150) =
            CASE WHEN @PaymentMode = 'Cash'
                 THEN 'Receipt No: ' + @StudentReceiptNo + '/dated:' + CONVERT(VARCHAR, @ReceiptDate, 103) + '/ By ' + @PaymentMode
                 ELSE 'Receipt No: ' + @StudentReceiptNo + '/dated:' + CONVERT(VARCHAR, @ReceiptDate, 103) + '/ By ' + @PaymentMode + ' cheque No:' + ISNULL(@ChequeNo, '')
            END;
        DECLARE @StResult INT = 0;
        DECLARE @tmpPaidAmount DECIMAL(18,2) = 0 - @PaidAmount;
        EXEC dbo.V3M_FIN_StudentTrans @GroupCode, @BranchCode, @SessionId, @StudentId, @StudentReceiptNo, 'Student Receipt', @Narration, @tmpPaidAmount, @PaymentMode, @ChequeNo, @TransDate, @CreatedBy, @StResult OUTPUT;

        ----------------------------------------------------------------------
        -- Student balance (direct-assignment convention, verbatim from the
        -- verified real procedure V3M_FIN_GeneratedStudentCounterAdjustemntReceipt)
        ----------------------------------------------------------------------
        DECLARE @NewBalance DECIMAL(18,2) = @Balance;
        IF @NewBalance > 0
            SET @NewBalance = 0;

        IF EXISTS (SELECT 1 FROM dbo.V3M_Fin_StudentBalance WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1)
            UPDATE dbo.V3M_Fin_StudentBalance SET Balance = @NewBalance WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;
        ELSE
            INSERT INTO dbo.V3M_Fin_StudentBalance (GroupCode, BranchCode, StudentId, Balance, CreatedDate, CreatedBy, SessionId, IsValid)
            VALUES (@GroupCode, @BranchCode, @StudentId, @NewBalance, GETDATE(), @CreatedBy, @SessionId, 1);

        ----------------------------------------------------------------------
        -- Journal hook (verbatim pattern - only fires when the school has
        -- opted into ledger accounting via DefaultSettings.NeedToManageAc)
        ----------------------------------------------------------------------
        DECLARE @NeedToManageAc VARCHAR(5), @ControlNo VARCHAR(30);
        IF EXISTS (SELECT 1 FROM dbo.DefaultSettings WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND KeyName = 'NeedToManageAc')
        BEGIN
            SELECT @NeedToManageAc = TRIM(ISNULL(KeyValue, '0')) FROM dbo.DefaultSettings WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND KeyName = 'NeedToManageAc';
            IF (@NeedToManageAc = '1')
            BEGIN
                SELECT TOP 1 @ControlNo = ControlNo FROM dbo.StudentMaster WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND StudentId = @StudentId;
                DECLARE @tmpNarration VARCHAR(500) = ISNULL(@Narration, '') + ' by Student No : ' + ISNULL(@ControlNo, '');
                EXEC dbo.V3M_Fin_ReceiptJournalEntry @GroupCode, @BranchCode, @SocietyId, @SessionId, @ReceiptDate, @PaymentMode, @PaidAmount, @tmpNarration, @CreatedBy, @BankAccountId, @ReceiptId;
            END
        END

        ----------------------------------------------------------------------
        -- Header-level audit trail (existing, previously-unused mechanism)
        ----------------------------------------------------------------------
        IF OBJECT_ID('dbo.Usp_SaveTransactionLog', 'P') IS NOT NULL AND TYPE_ID('dbo.ColumnChangeType') IS NOT NULL
        BEGIN
            DECLARE @AuditChanges dbo.ColumnChangeType;
            INSERT INTO @AuditChanges (ColumnName, OldValue, NewValue)
            VALUES
                ('ReceiptNo', NULL, @StudentReceiptNo),
                ('PaidAmount', NULL, CAST(@PaidAmount AS NVARCHAR(50))),
                ('PaymentMode', NULL, @PaymentMode),
                ('StudentId', NULL, CAST(@StudentId AS NVARCHAR(50)));

            EXEC dbo.Usp_SaveTransactionLog
                @ReferenceKey  = 'V3M_FIN_StudentReceiptMaster',
                @ReferenceId   = @StudentReceiptNo,
                @TableName     = 'V3M_FIN_StudentReceiptMaster',
                @OperationType = 'INSERT',
                @TransBy       = @CreatedBy,
                @IP            = NULL,
                @Changes       = @AuditChanges;
        END

        COMMIT TRANSACTION;
        SET @ErrorCode = 0;
        RETURN @ErrorCode;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        INSERT INTO dbo.Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Finance', 'FeeCollection', 'V3M_FIN_AdjustStudentFeeHeadWise_Fast', ERROR_MESSAGE() + ' Line:' + CAST(ERROR_LINE() AS VARCHAR(10)), GETDATE());

        THROW;
    END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_AdjustStudentFeeHeadWise_Fast - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_CancelStudentReceipt

ALTER PROCEDURE dbo.V3M_FIN_CancelStudentReceipt
    @ReceiptId      BIGINT,
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @CancelledBy    VARCHAR(50),
    @CancelReason   VARCHAR(250) = '',
    @CancelType     TINYINT = 1,   -- 1 = Cancelled, 2 = Edited (superseded)
    @Result         INT OUTPUT
AS
BEGIN
    ------------------------------ Affected Tables -------------------------------------
    -- V3M_FIN_StudentReceiptMaster (Valid/CancelBy/CancelDate/CancelType set)
    -- V3M_FIN_DetInvoice, V3M_FIN_MstInvoice (payment reversed)
    -- V3M_FIN_FeeHeadLedger, V3M_FIN_StudentLedger (reversal rows appended)
    -- V3M_Fin_StudentBalance (restored to PreviousBalance)
    -- Det_TransactionLog (audit, via Usp_SaveTransactionLog)
    -------------------------------------------------------------------------------------
    SET NOCOUNT ON;
    SET @Result = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster
            WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND Valid = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SET @Result = -1;   -- receipt not found, or already cancelled
            RETURN -1;
        END

        DECLARE @StudentId BIGINT, @SessionId BIGINT, @ReceiptNo VARCHAR(50), @PreviousBalance DECIMAL(18,2);
        SELECT @StudentId = StudentId, @SessionId = SessionId, @ReceiptNo = ReceiptNo, @PreviousBalance = PreviousBalance
        FROM dbo.V3M_FIN_StudentReceiptMaster
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        ----------------------------------------------------------------------
        -- Reverse V3M_FIN_DetInvoice for every line this receipt paid
        -- (matched the same way FIN_Usp_RemoveFeeHeadFromChallan matches lines:
        --  InvoiceId + FeeHeadId + MonthNo, since ReceiptDetail has no DetInvoiceId column)
        ----------------------------------------------------------------------
        UPDATE di
        SET di.ReceivedAmt = ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount,
            di.UpdatedBy   = @CancelledBy,
            di.UpdatedDate = GETDATE(),
            di.Status = CASE
                WHEN (ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount) <= 0 THEN 1
                WHEN (ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount + ISNULL(di.Concession, 0) + ISNULL(di.WaiveOff, 0)) >= di.Amount THEN 3
                ELSE 2
            END
        FROM dbo.V3M_FIN_DetInvoice di
        INNER JOIN dbo.V3M_FIN_StudentReceiptDetail rd
            ON rd.InvoiceId = di.InvoiceId AND rd.FeeHeadId = di.FeeHeadId AND rd.MonthNo = di.MonthNo
        WHERE rd.ReceiptId = @ReceiptId AND rd.InvoiceId IS NOT NULL AND rd.Valid = 1;

        ----------------------------------------------------------------------
        -- Roll back into V3M_FIN_MstInvoice
        ----------------------------------------------------------------------
        ;WITH InvoiceTotals AS (
            SELECT InvoiceId, SUM(PaidAmount) AS PaidSum
            FROM dbo.V3M_FIN_StudentReceiptDetail
            WHERE ReceiptId = @ReceiptId AND InvoiceId IS NOT NULL AND Valid = 1
            GROUP BY InvoiceId
        )
        UPDATE mi
        SET mi.ReceivedAmount = ISNULL(mi.ReceivedAmount, 0) - it.PaidSum,
            mi.ActualReceived = ISNULL(mi.ActualReceived, 0) - it.PaidSum,
            mi.Ch_Status = CASE WHEN ISNULL(mi.ReceivedAmount, 0) - it.PaidSum > 0 THEN 2 ELSE 1 END
        FROM dbo.V3M_FIN_MstInvoice mi
        INNER JOIN InvoiceTotals it ON it.InvoiceId = mi.InvoiceId;

        ----------------------------------------------------------------------
        -- Mirror-reverse every FeeHeadLedger row this receipt created
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, -Amount, RefType, RefId, TransType,
            'Reversal (Cancel Receipt ' + @ReceiptNo + ') - ' + ISNULL(Narration, ''), 1, InvoiceId, GETDATE(), @CancelledBy, ReceiptId, IsReceiptOnline, CrDr, 'Pro_V3M_FIN_CancelStudentReceipt', SessionId
        FROM dbo.V3M_FIN_FeeHeadLedger
        WHERE ReceiptId = @ReceiptId AND IsValid = 1;

        ----------------------------------------------------------------------
        -- Mirror-reverse the StudentLedger entry this receipt created
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_StudentLedger
            (GroupCode, BranchCode, SessionId, StudentId, ReferenceNo, ReferenceType, Narration, Amount, TransMode, TransDetail, RunningBalance, TransactionDate, TransactionBy, CreatedDate)
        SELECT
            GroupCode, BranchCode, SessionId, StudentId, ReferenceNo, ReferenceType,
            'Reversal (Cancel Receipt ' + @ReceiptNo + ') - ' + ISNULL(Narration, ''), -Amount, TransMode, TransDetail,
            dbo.Fn_GetStudentRunningBalance(GroupCode, BranchCode, SessionId, StudentId, -Amount), GETDATE(), @CancelledBy, GETDATE()
        FROM dbo.V3M_FIN_StudentLedger
        WHERE ReferenceNo = @ReceiptNo AND StudentId = @StudentId;

        ----------------------------------------------------------------------
        -- Restore V3M_Fin_StudentBalance to its value before this receipt
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_Fin_StudentBalance
        SET Balance = @PreviousBalance
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;

        ----------------------------------------------------------------------
        -- Mark the receipt cancelled (activates existing, previously-unused columns)
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_FIN_StudentReceiptMaster
        SET Valid = 0,
            CancelBy = @CancelledBy,
            CancelDate = GETDATE(),
            CancelType = @CancelType,
            Remark = ISNULL(Remark, '') + ' | Cancelled: ' + ISNULL(@CancelReason, '')
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        UPDATE dbo.V3M_FIN_StudentReceiptDetail
        SET Valid = 0
        WHERE ReceiptId = @ReceiptId;

        ----------------------------------------------------------------------
        -- Header-level audit trail
        ----------------------------------------------------------------------
        IF OBJECT_ID('dbo.Usp_SaveTransactionLog', 'P') IS NOT NULL AND TYPE_ID('dbo.ColumnChangeType') IS NOT NULL
        BEGIN
            DECLARE @AuditChanges dbo.ColumnChangeType;
            INSERT INTO @AuditChanges (ColumnName, OldValue, NewValue)
            VALUES
                ('Valid', '1', '0'),
                ('CancelType', NULL, CAST(@CancelType AS NVARCHAR(10))),
                ('CancelReason', NULL, @CancelReason);

            DECLARE @OpType VARCHAR(20) = CASE WHEN @CancelType = 2 THEN 'EDIT-CANCEL' ELSE 'CANCEL' END;
            EXEC dbo.Usp_SaveTransactionLog
                @ReferenceKey  = 'V3M_FIN_StudentReceiptMaster',
                @ReferenceId   = @ReceiptNo,
                @TableName     = 'V3M_FIN_StudentReceiptMaster',
                @OperationType = @OpType,
                @TransBy       = @CancelledBy,
                @IP            = NULL,
                @Changes       = @AuditChanges;
        END

        COMMIT TRANSACTION;
        SET @Result = 1;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        INSERT INTO dbo.Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Finance', 'FeeCollection', 'V3M_FIN_CancelStudentReceipt', ERROR_MESSAGE() + ' Line:' + CAST(ERROR_LINE() AS VARCHAR(10)), GETDATE());

        SET @Result = 0;
        THROW;
    END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_CancelStudentReceipt - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_InsertCounterReceiptDetails
ALTER PROCEDURE [dbo].[V3M_FIN_InsertCounterReceiptDetails]
(
    @StudentId        BIGINT,
    @GroupCode        VARCHAR(10),
    @BranchCode       VARCHAR(10),
    @SessionId        BIGINT,
    @ClassCode        VARCHAR(20),
    @SectionId        INT,
    @PaidAmount       DECIMAL(18,2),
    @Balance          DECIMAL(18,2),
    @PaymentMode      VARCHAR(10),
    @ChequeNo         VARCHAR(50) = '',
    @ChequeDate       DATE = NULL,
    @ChequeBank       VARCHAR(80) = '',
    @ChequeType       VARCHAR(20) = '',
    @LateFee          DECIMAL(18,2) = 0,
    @PreviousBalance  DECIMAL(18,2),
    @Remark           VARCHAR(250) = '',
    @SocietyId        INT = 0,
    @CreatedBy        VARCHAR(20),
    @ReceiptDate      DATE,
    @BankAccountId    INT,
    @BankBranch       VARCHAR(100) = '',
    @FeeHeadDetails   dbo.FeeHeadDetailType READONLY,
    @StudentReceiptNo VARCHAR(50) OUTPUT,
    @ReceiptId        BIGINT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @StudentId IS NULL OR @StudentId = 0
            RETURN;

        DECLARE @LumpsumTotal DECIMAL(18,2), @TotalAmount DECIMAL(18,2), @TotalConcession DECIMAL(18,2);

        SELECT
            @LumpsumTotal = ISNULL(SUM(CASE WHEN StatusFlag = '2' THEN LumpsumAmount ELSE 0 END), 0),
            @TotalAmount  = ISNULL(SUM(CASE
                                WHEN StatusFlag = '2' THEN 0
                                WHEN AdjustAmount > 0 THEN (FeeHeadAmount - Deduction - AdjustAmount - Concession)
                                ELSE (FeeHeadAmount - Deduction - AdjustAmount) END), 0),
            @TotalConcession = ISNULL(SUM(CASE
                                WHEN StatusFlag = '2' THEN 0
                                WHEN AdjustAmount > 0 THEN 0
                                ELSE Concession END), 0)
        FROM @FeeHeadDetails;

        DECLARE @FinalPreviousBalance DECIMAL(18,2);

        IF @PreviousBalance < 0 AND @LumpsumTotal > 0
            RETURN;
        ELSE IF @LumpsumTotal <= 0
            SET @FinalPreviousBalance = @PreviousBalance;
        ELSE
            SET @FinalPreviousBalance = @LumpsumTotal;

        BEGIN TRANSACTION;

        EXEC dbo.V3M_FIN_GeneratedStudentCounterAdjustemntReceipt
            @StudentId = @StudentId, @GroupCode = @GroupCode, @BranchCode = @BranchCode,
            @SessionId = @SessionId, @ClassCode = @ClassCode, @SectionId = @SectionId,
            @TotalAmount = @TotalAmount, @PaidAmount = @PaidAmount, @Concession = @TotalConcession,
            @Balance = @Balance, @PaymentMode = @PaymentMode, @ChequeNo = @ChequeNo,
            @ChequeDate = @ChequeDate, @ChequeBank = @ChequeBank, @LateFee = @LateFee,
            @PreviousBalance = @FinalPreviousBalance, @Remark = @Remark, @ChequeType = @ChequeType,
            @SocietyId = @SocietyId, @CreatedBy = @CreatedBy, @ReceiptDate = @ReceiptDate,
            @BankAccountId = @BankAccountId, @BankBranch = @BankBranch,
            @StudentReceiptNo = @StudentReceiptNo OUTPUT, @ReceiptId = @ReceiptId OUTPUT;

        IF @StudentReceiptNo IS NULL OR LTRIM(RTRIM(@StudentReceiptNo)) = ''
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END

        --=================================================================
        -- Ek hi step: month-split + invoice-match seedha @Matched mein
        -- (pehle wale @Expanded temp-table ki zarurat nahi)
        --=================================================================
        DECLARE @Matched TABLE
        (
            FeeHeadId     INT,
            MonthNo       INT,
            PaidAmount    DECIMAL(18,2),
            FeeHeadAmount DECIMAL(18,2),
            Concession    DECIMAL(18,2),
            IsAdjusted    INT,
            InvoiceId     BIGINT,
            DetInvoiceId  BIGINT
        );

        INSERT INTO @Matched (FeeHeadId, MonthNo, PaidAmount, FeeHeadAmount, Concession, IsAdjusted, InvoiceId, DetInvoiceId)
        SELECT
            fhd.FeeHeadId,
            e.MonthNo,
            fhd.PaidAmount,
            CASE WHEN fhd.StatusFlag = '2' OR fhd.AdjustAmount > 0 THEN fhd.LumpsumAmount ELSE (fhd.FeeHeadAmount - fhd.Deduction) END,
            CASE WHEN fhd.StatusFlag = '2' OR fhd.AdjustAmount > 0 THEN 0 ELSE fhd.Concession END,
            CASE WHEN fhd.StatusFlag = '2' THEN 1 ELSE 0 END,
            x.InvoiceId,
            x.DetInvoiceId
        FROM @FeeHeadDetails fhd
        CROSS APPLY (SELECT CAST(m.value AS INT) AS MonthNo FROM STRING_SPLIT(fhd.MonthNoCsv, ',') m) e
        CROSS APPLY (
            SELECT TOP 1 mcn.InvoiceId, dcn.DetInvoiceId
            FROM V3M_FIN_MstInvoice mcn
            INNER JOIN V3M_FIN_DetInvoice dcn ON mcn.InvoiceId = dcn.InvoiceId
            WHERE mcn.GroupCode = @GroupCode AND mcn.BranchCode = @BranchCode
              AND mcn.SessionId = @SessionId AND mcn.StudentId = @StudentId
              AND dcn.FeeHeadId = fhd.FeeHeadId
              AND ((fhd.DetInvoiceId > 0 AND dcn.DetInvoiceId = fhd.DetInvoiceId) OR (fhd.DetInvoiceId = 0 AND dcn.MonthNo = e.MonthNo))
              AND mcn.Ch_Status > 0
              AND dcn.ReceivedAmt < (dcn.Amount - dcn.Concession - ISNULL(dcn.WaiveOff, 0))
        ) x;

        --=================================================================
        -- Receipt-detail - ek bulk INSERT
        --=================================================================
        INSERT INTO V3M_FIN_StudentReceiptDetail
            (ReceiptNo, ReceiptId, InvoiceId, FeeHeadId, MonthNo, PaidAmount, FeeHeadAmount, FeeHeadConcession, Valid, CreatedBy, CreatedDate, GroupCode, BranchCode, IsAdjusted)
        SELECT
            @StudentReceiptNo, @ReceiptId, m.InvoiceId, m.FeeHeadId, m.MonthNo, m.PaidAmount, m.FeeHeadAmount, m.Concession, 1, @CreatedBy, GETDATE(), @GroupCode, @BranchCode, m.IsAdjusted
        FROM @Matched m;

        --=================================================================
        -- DetInvoice update (NULL-safe)
        --=================================================================
        UPDATE dcn
        SET dcn.ReceivedAmt = ISNULL(dcn.ReceivedAmt, 0) + agg.TotalPaid,
            dcn.ReceivedDate = GETDATE(),
            dcn.Status = CASE WHEN (dcn.Amount - dcn.Concession - (ISNULL(dcn.ReceivedAmt, 0) + agg.TotalPaid)) <= 0 THEN 3 ELSE 2 END
        FROM V3M_FIN_DetInvoice dcn
        INNER JOIN (SELECT DetInvoiceId, SUM(PaidAmount) AS TotalPaid FROM @Matched GROUP BY DetInvoiceId) agg
            ON dcn.DetInvoiceId = agg.DetInvoiceId;

        --=================================================================
        -- MstInvoice update (NULL-safe) + Ch_Status ek hi pass mein
        --=================================================================
        UPDATE mcn
        SET mcn.ReceivedAmount = ISNULL(mcn.ReceivedAmount, 0) + agg.TotalPaid,
            mcn.ActualReceived = ISNULL(mcn.ActualReceived, 0) + agg.TotalPaid,
            mcn.Ch_Status = CASE WHEN bal.Balance <= 0 THEN 3 ELSE 2 END
        FROM V3M_FIN_MstInvoice mcn
        INNER JOIN (SELECT InvoiceId, SUM(PaidAmount) AS TotalPaid FROM @Matched GROUP BY InvoiceId) agg
            ON mcn.InvoiceId = agg.InvoiceId
        INNER JOIN (
            SELECT InvoiceId, SUM(Amount) - (SUM(Concession) + SUM(ReceivedAmt)) AS Balance
            FROM V3M_FIN_DetInvoice
            WHERE InvoiceId IN (SELECT DISTINCT InvoiceId FROM @Matched)
            GROUP BY InvoiceId
        ) bal ON mcn.InvoiceId = bal.InvoiceId
        WHERE mcn.GroupCode = @GroupCode AND mcn.BranchCode = @BranchCode;

        --=================================================================
        -- Ledger entries - ek bulk INSERT
        --=================================================================
        INSERT INTO V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType,
             Narration, IsValid, InvoiceId, TransDate, TransBy, CreatedDate, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            @GroupCode, @BranchCode, @SessionId, m.FeeHeadId, m.MonthNo, MONTH(GETDATE()), @SessionId, -m.PaidAmount, 'STUDENT', @StudentId, 'RECV',
            'Receipt for Month No - ' + CAST(m.MonthNo AS VARCHAR), 1, m.InvoiceId, @ReceiptDate, @CreatedBy, GETDATE(), @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_AdjustStudentFeeHeadWise_Fast', @SessionId
        FROM @Matched m;

        --=================================================================
        -- Refundable liabilities - ek bulk INSERT
        --=================================================================
        INSERT INTO V3M_FIN_StudentLibilities (StudentId, HeadId, Amount, Date, Session, ReceiptNo, PaidStatus, GroupCode, BranchCode, CreatedBy, Remarks)
        SELECT @StudentId, m.FeeHeadId, m.PaidAmount, GETDATE(), @SessionId, @StudentReceiptNo, 1, @GroupCode, @BranchCode, @CreatedBy, 'Student Receipt'
        FROM @Matched m
        WHERE EXISTS (
            SELECT 1 FROM v3m_fin_mstfeehead fh
            WHERE fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
              AND fh.FeeheadId = m.FeeHeadId AND fh.FeeHeadType = 'Refundable'
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES('Procedure','V3M_FIN_AdjustStudentFeeHeadWise_Fast','AdjustStudentFeeHeadWise_Fast',ERROR_MESSAGE(),GETDATE());

        THROW;
    END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_InsertCounterReceiptDetails - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_InsertAdmissionReceiptDetails

ALTER PROCEDURE [dbo].[V3M_FIN_InsertAdmissionReceiptDetails]
    @GroupCode        VARCHAR(10),
    @BranchCode       VARCHAR(10),
    @StudentId        BIGINT,
    @SessionId        BIGINT,          -- current session
    @AdmissionSession VARCHAR(50),
    @IsBankProcess    INT = 0,
    @ClassCode        VARCHAR(20),
    @ReceiptNo        VARCHAR(50),
    @ReceiptId        BIGINT,
    @TotalPaidAmount  DECIMAL(18,2),   -- C# ka num (loop se pehle ki value)
    @CreatedBy        VARCHAR(20),
    @FeeHeads         dbo.TVP_AdmissionReceiptFeeHead READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- C# ka transaction hi use hoga; error par poora rollback
    DECLARE @InvoiceId BIGINT = 0,
            @Now       DATETIME = GETDATE(),
            @MonthNow  INT = MONTH(GETDATE()),
            @TotalPaid DECIMAL(18,2) = 0;
    IF NOT EXISTS (SELECT 1 FROM @FeeHeads)
        RETURN 0;
    ------------------------------------------------------------------
    -- 1. Invoice: isi receipt ke saath bana invoice (receipt master se)
    ------------------------------------------------------------------
    SELECT @InvoiceId = InvoiceId
    FROM V3M_FIN_StudentReceiptMaster
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

    IF ISNULL(@InvoiceId, 0) = 0
        SELECT TOP 1 @InvoiceId = InvoiceId
        FROM V3M_FIN_MstInvoice
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode
          AND StudentId = @StudentId AND ClassCode = @ClassCode AND Ch_Status > 0
        ORDER BY InvoiceId DESC;

    IF ISNULL(@InvoiceId, 0) = 0
        THROW 50001, 'Admission invoice not found for this receipt.', 1;

    ------------------------------------------------------------------
    -- 2. Paid amount ka allocation (C# ke num/num2 loop jaisa)
    --    Har row ko: min(PayableAmount, bacha hua amount), 0 se kam nahi
    ------------------------------------------------------------------
    CREATE TABLE #Rows
    (
        RowNo         INT PRIMARY KEY,
        FeeHeadId     INT,
        MonthNo       INT,
        FeeHeadAmount DECIMAL(18,2),
        ConcessionAmt DECIMAL(18,2),
        PaidAmount    DECIMAL(18,2),
        DetInvoiceId  BIGINT NULL
    );

    INSERT INTO #Rows (RowNo, FeeHeadId, MonthNo, FeeHeadAmount, ConcessionAmt, PaidAmount)
    SELECT x.RowNo, x.FeeHeadId, x.MonthNo, x.FeeHeadAmount, x.ConcessionAmt,
           CASE WHEN x.Remaining <= 0              THEN 0
                WHEN x.Remaining > x.PayableAmount THEN x.PayableAmount
                ELSE x.Remaining END
    FROM
    (
        SELECT f.RowNo, f.FeeHeadId, f.MonthNo, f.PayableAmount, f.FeeHeadAmount,
               ISNULL(f.ConcessionAmt, 0) AS ConcessionAmt,
               @TotalPaidAmount
               - ISNULL(SUM(f.PayableAmount) OVER (ORDER BY f.RowNo
                         ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS Remaining
        FROM @FeeHeads f
    ) x;

    -- DetInvoiceId: concurrency-safe sequence (Max+1 ki jagah)
    UPDATE #Rows SET DetInvoiceId = NEXT VALUE FOR dbo.Seq_V3M_FIN_DetInvoiceId;

    ------------------------------------------------------------------
    -- 3. Receipt detail
    ------------------------------------------------------------------
    INSERT INTO V3M_FIN_StudentReceiptDetail
        (ReceiptNo, ReceiptId, InvoiceId, FeeHeadId, MonthNo, PaidAmount, FeeHeadConcession,
         FeeHeadAmount, Valid, CreatedBy, CreatedDate, GroupCode, BranchCode)
    SELECT @ReceiptNo, @ReceiptId, @InvoiceId, r.FeeHeadId, r.MonthNo, r.PaidAmount, r.ConcessionAmt,
           r.FeeHeadAmount, 1, @CreatedBy, @Now, @GroupCode, @BranchCode
    FROM #Rows r;

    ------------------------------------------------------------------
    -- 4. Invoice detail (status seedha calculate: 3 = full, 2 = partial, 1 = pending)
    ------------------------------------------------------------------
    INSERT INTO V3M_FIN_DetInvoice
        (DetInvoiceId, InvoiceId, FeeHeadId, DisplayOrder, MonthNo, Amount, Concession,
         ReceivedAmt, WaiveOff, IsValid, [Status])
    SELECT r.DetInvoiceId, @InvoiceId, r.FeeHeadId, 0, r.MonthNo, r.FeeHeadAmount, r.ConcessionAmt,
           r.PaidAmount, 0, 1,
           CASE WHEN r.ConcessionAmt + r.PaidAmount = r.FeeHeadAmount THEN 3
                WHEN r.ConcessionAmt + r.PaidAmount > 0
                 AND r.ConcessionAmt + r.PaidAmount < r.FeeHeadAmount THEN 2
                ELSE 1 END
    FROM #Rows r;

    ------------------------------------------------------------------
    -- 5. Fee head ledger: DUE, CON, RECV
    ------------------------------------------------------------------
    INSERT INTO V3M_FIN_FeeHeadLedger
        (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount,
         RefType, RefId, TransType, TransDate, Narration, IsValid, InvoiceId, TransBy, ReceiptId,
         IsReceiptOnline, CrDr, EnteredVia, CreatedDate, SessionId)
    SELECT @GroupCode, @BranchCode, @SessionId, r.FeeHeadId, r.MonthNo, @MonthNow, @AdmissionSession,
           r.FeeHeadAmount, 'STUDENT', @StudentId, 'DUE', @Now, 'Admission Challan', 1, @InvoiceId,
           @CreatedBy, @ReceiptId, 0, 'DR', 'Pro_V3M_FIN_InsertAdmissionReceiptDetails', @Now, @SessionId
    FROM #Rows r

    UNION ALL

    SELECT @GroupCode, @BranchCode, @SessionId, r.FeeHeadId, r.MonthNo, @MonthNow, @AdmissionSession,
           -r.ConcessionAmt, 'STUDENT', @StudentId, 'CON', @Now, 'Admission Challan Concession', 1, @InvoiceId,
           @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_InsertAdmissionReceiptDetails', @Now, @SessionId
    FROM #Rows r
    WHERE r.ConcessionAmt > 0

    UNION ALL

    SELECT @GroupCode, @BranchCode, @SessionId, r.FeeHeadId, r.MonthNo, @MonthNow, @AdmissionSession,
           -r.PaidAmount, 'STUDENT', @StudentId, 'RECV', @Now, 'Offline Received', 1, @InvoiceId,
           @CreatedBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_InsertAdmissionReceiptDetails', @Now, @SessionId
    FROM #Rows r
    WHERE r.PaidAmount > 0;

    ------------------------------------------------------------------
    -- 6. Master invoice: received amount aur status (ek hi baar)
    ------------------------------------------------------------------
    SELECT @TotalPaid = ISNULL(SUM(PaidAmount), 0) FROM #Rows;

    IF @TotalPaid > 0
        UPDATE V3M_FIN_MstInvoice
        SET ReceivedAmount = ISNULL(ReceivedAmount, 0) + @TotalPaid
        WHERE InvoiceId = @InvoiceId;

    UPDATE V3M_FIN_MstInvoice
    SET Ch_Status = CASE
            WHEN (ISNULL(TotalAmount,0) + ISNULL(ArrearAmount,0) + ISNULL(LateCharges,0))
               - (ISNULL(TotalConcession,0) + ISNULL(ReceivedAmount,0) + ISNULL(ArrearAmount,0) + ISNULL(TotalWaiveOff,0)) <= 0
            THEN 3 ELSE 2 END
    WHERE InvoiceId = @InvoiceId;

    ------------------------------------------------------------------
    -- 7. Student-specific fee heads
    ------------------------------------------------------------------
    SELECT DISTINCT r.FeeHeadId
    INTO #SpecificHeads
    FROM #Rows r
    INNER JOIN V3M_FIN_DetFeeHeadMappedToClass c
        ON c.FeeHeadId = r.FeeHeadId AND c.SessionId = @AdmissionSession
       AND c.ClassCode = @ClassCode AND c.IsValid = 1 AND ISNULL(c.IsStudentSpecific, 0) = 1;

    UPDATE s SET s.IsValid = 1
    FROM V3M_FIN_DetFeeHeadMappedToStudent s
    INNER JOIN #SpecificHeads h ON h.FeeHeadId = s.FeeHeadId
    WHERE s.GroupCode = @GroupCode AND s.BranchCode = @BranchCode
      AND s.StudentId = @StudentId AND s.SessionId = @AdmissionSession;

    INSERT INTO V3M_FIN_DetFeeHeadMappedToStudent
        (GroupCode, BranchCode, StudentId, FeeHeadId, SessionId, CreatedDate, CreatedBy, IsValid, ApplicableFrom)
    SELECT @GroupCode, @BranchCode, @StudentId, h.FeeHeadId, @AdmissionSession, @Now, @CreatedBy, 1, @Now
    FROM #SpecificHeads h
    WHERE NOT EXISTS (SELECT 1 FROM V3M_FIN_DetFeeHeadMappedToStudent s
                      WHERE s.GroupCode = @GroupCode AND s.BranchCode = @BranchCode
                        AND s.StudentId = @StudentId AND s.FeeHeadId = h.FeeHeadId
                        AND s.SessionId = @AdmissionSession);

    ------------------------------------------------------------------
    -- 8. Refundable fee heads -> student liabilities
    ------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM #Rows r
               INNER JOIN V3M_FIN_MstFeeHead fh
                   ON fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
                  AND fh.FeeHeadId = r.FeeHeadId AND fh.FeeHeadType = 'Refundable')
    BEGIN
        DECLARE @LibSessionId BIGINT = @SessionId;
        IF @LibSessionId = 0
            SELECT @LibSessionId = SessionId FROM V3M_FIN_StudentReceiptMaster
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

        INSERT INTO V3M_FIN_StudentLibilities
            (StudentId, HeadId, Amount, [Date], [Session], ReceiptNo, PaidStatus, GroupCode, BranchCode, CreatedBy, Remarks)
        SELECT @StudentId, r.FeeHeadId, r.PaidAmount, @Now, @LibSessionId, @ReceiptNo, 1,
               @GroupCode, @BranchCode, @CreatedBy, 'Student Receipt'
        FROM #Rows r
        INNER JOIN V3M_FIN_MstFeeHead fh
            ON fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
           AND fh.FeeHeadId = r.FeeHeadId AND fh.FeeHeadType = 'Refundable';
    END

    RETURN 0;
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_InsertAdmissionReceiptDetails - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_UspApplyStudentConcessionOnChallan
ALTER PROC [dbo].[V3M_FIN_UspApplyStudentConcessionOnChallan]
    @GroupCode varchar(10), 
    @BranchCode varchar(10),
    @SessionId bigint,
    @InvoiceId bigint, 
    @CreatedBy varchar(50), 
    @Narration varchar(250),
    @StudentId bigint = NULL,
    @ConcessionIds varchar(50) = NULL,      
    @AppliedConcessionIds varchar(50) = NULL OUTPUT,
    @SkippedConcessionIds varchar(50) = NULL OUTPUT,
    @ResultMessage varchar(1000) = NULL OUTPUT,   -- ?? naya: readable message
    @ResultValue int output
AS
    DECLARE @InvoiceNo varchar(50)
    DECLARE @InvoiceStudentId bigint
    DECLARE @ConcessionId int
    DECLARE @ConcessionName varchar(50)
    DECLARE @ConcessionDisplayName varchar(50)      -- ?? naya: message ke liye readable naam
    DECLARE @ConcessionFromDate date
    DECLARE @ConcessionToDate date
    DECLARE @ClassCode varchar(50)
    DECLARE @FeeheadId int
    DECLARE @FeeHeadAmount decimal(18,2) = 0
    DECLARE @ConcessionAmount decimal(18,2) = 0
    DECLARE @ChallanConcessionAmount decimal(18,2) = 0
    DECLARE @ChallanWaiveOffAmount decimal(18,2) = 0
    DECLARE @BalanceConcession decimal(18,2) = 0
    DECLARE @TotalConcessionAmount decimal(18,2) = 0
    DECLARE @TotalAdvanceAmount decimal(18,2) = 0
    DECLARE @MonthNo int
    DECLARE @MonthEndDate date
    DECLARE @IsRoundOff int = 0
    DECLARE @Advance decimal(18,2), @tmpHeadAmt decimal(18,2), @tmpReceivedAmt decimal(18,2), @tmpWaiveOffAmt decimal(18,2), @tmpConcession decimal(18,2)
    DECLARE @tmpchBalance decimal(18,2) = 0
    DECLARE @RequestedConcessionCount int = 0
    DECLARE @ConcessionAmountBeforeThis decimal(18,2)
    DECLARE @ThisConcessionApplied decimal(18,2)    -- ?? naya: is concession ne kitna apply kiya

BEGIN
 BEGIN TRY
  BEGIN TRAN
    SET @ResultValue = 0
    SET @AppliedConcessionIds = NULL
    SET @SkippedConcessionIds = NULL
    SET @ResultMessage = NULL

    IF @ConcessionIds IS NOT NULL AND TRIM(@ConcessionIds) = ''
        SET @ConcessionIds = NULL;

    IF NOT EXISTS(SELECT * FROM V3M_FIN_MstInvoice WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND InvoiceId=@InvoiceId)
    BEGIN
        SET @ResultValue = 1
        SET @ResultMessage = 'Invoice not found.'
    END
    ELSE IF NOT EXISTS(SELECT * FROM V3M_FIN_MstInvoice WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND InvoiceId=@InvoiceId AND Ch_Status > 0)
    BEGIN
        SET @ResultValue = 2
        SET @ResultMessage = 'Invoice is not in a valid status for concession.'
    END
    ELSE IF EXISTS(SELECT * FROM V3M_FIN_MstInvoice WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND InvoiceId=@InvoiceId AND Ch_Status > 0 AND IsActive = 1 AND TotalConcession > 0)
    BEGIN
        SET @ResultValue = 5
        SET @ResultMessage = 'Concession already applied on this invoice.'
    END

    IF @ResultValue = 0 AND EXISTS(SELECT * FROM V3M_FIN_MstInvoice WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND InvoiceId=@InvoiceId AND Ch_Status > 0 AND ReceivedAmount = 0)
    BEGIN
        SELECT @InvoiceStudentId = StudentId, @ClassCode = ClassCode, @InvoiceNo = InvoiceNo
        FROM V3M_FIN_MstInvoice
        WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND InvoiceId=@InvoiceId AND Ch_Status > 0 AND ReceivedAmount = 0

        IF @StudentId IS NOT NULL AND @StudentId <> @InvoiceStudentId
        BEGIN
            SET @ResultValue = 7
            SET @ResultMessage = 'Given StudentId does not match the invoice.'
        END
        ELSE
        BEGIN
            SET @StudentId = @InvoiceStudentId
            SET @TotalConcessionAmount = 0
            SET @TotalAdvanceAmount = 0

            IF @ConcessionIds IS NOT NULL AND LTRIM(RTRIM(@ConcessionIds)) <> ''
            BEGIN
                SELECT @RequestedConcessionCount = COUNT(*)
                FROM STRING_SPLIT(@ConcessionIds, ',')
                WHERE LTRIM(RTRIM(value)) <> ''

                IF @RequestedConcessionCount > 2
                BEGIN
                    SET @ResultValue = 8
                    SET @ResultMessage = 'Maximum 2 concessions can be applied at once.'
                END
            END

            IF @ResultValue = 0
            BEGIN
                IF EXISTS(SELECT * FROM V3M_FIN_DetConcessionMapToStudent WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND SessionId=@SessionId AND ClassCode=@ClassCode AND StudentId=@StudentId AND Status=1)
                BEGIN
                    DECLARE ConcessionMapCursor CURSOR LOCAL FAST_FORWARD FOR
                        SELECT TOP 2 dcs.ConcessionId, dcs.ValidFrom, dcs.ValidUpto
                        FROM V3M_FIN_DetConcessionMapToStudent dcs
                        WHERE dcs.GroupCode = @GroupCode AND dcs.BranchCode = @BranchCode AND dcs.SessionId = @SessionId
                          AND dcs.ClassCode = @ClassCode AND dcs.Status = 1 AND dcs.StudentId = @StudentId
                          AND (
                                @ConcessionIds IS NULL OR LTRIM(RTRIM(@ConcessionIds)) = ''
                                OR dcs.ConcessionId IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ConcessionIds, ',') WHERE LTRIM(RTRIM(value)) <> '')
                              )
                        ORDER BY dcs.ConcStudId ASC

                    OPEN ConcessionMapCursor
                    FETCH NEXT FROM ConcessionMapCursor INTO @ConcessionId, @ConcessionFromDate, @ConcessionToDate

                    IF @@FETCH_STATUS <> 0 AND @ConcessionIds IS NOT NULL AND LTRIM(RTRIM(@ConcessionIds)) <> ''
                    BEGIN
                        CLOSE ConcessionMapCursor
                        DEALLOCATE ConcessionMapCursor
                        SET @ResultValue = 9
                        SET @ResultMessage = 'None of the given ConcessionIds are active for this student/class.'
                    END
                    ELSE
                    BEGIN
                        WHILE @@FETCH_STATUS = 0
                        BEGIN
                            SET @ConcessionAmountBeforeThis = @TotalConcessionAmount

                            -- ?? naya: message ke liye concession ka display-naam nikalo
                            SELECT @ConcessionDisplayName = Concession FROM V3M_FIN_MstConcession WHERE ConcessionId = @ConcessionId

                            DECLARE CurrFeeMonth CURSOR LOCAL FAST_FORWARD FOR
                                SELECT FeeHeadId, MonthNo, Amount, Concession, ISNULL(WaiveOff,0),
                                    DATEADD(DAY,-1, DATEADD(MONTH,1, DATEFROMPARTS(
                                        CASE WHEN MonthNo > 3 THEN YEAR(GETDATE()) ELSE YEAR(GETDATE()) + 1 END, MonthNo, 1 ) ) ) AS DueDate
                                FROM V3M_FIN_DetInvoice WHERE InvoiceId = @InvoiceId AND IsValid = 1 ORDER BY DisplayOrder

                            OPEN CurrFeeMonth
                            FETCH NEXT FROM CurrFeeMonth INTO @FeeHeadId, @MonthNo, @FeeHeadAmount, @ChallanConcessionAmount, @ChallanWaiveOffAmount, @MonthEndDate

                            WHILE @@FETCH_STATUS = 0
                            BEGIN
                                SET @ConcessionAmount = 0
                                SET @BalanceConcession = 0

                                IF (@ConcessionId <> 0 AND @ConcessionFromDate IS NULL) OR (@ConcessionId <> 0 AND @MonthEndDate BETWEEN @ConcessionFromDate AND @ConcessionToDate)
                                BEGIN
                                    SELECT @ConcessionName = Type FROM V3M_FIN_MstConcession WHERE ConcessionId = @ConcessionId

                                    IF (@ConcessionName = 'Student')
                                    BEGIN
                                        SELECT @ConcessionAmount = CASE ConcessionType WHEN 0 THEN (@FeeHeadAmount * ConcessionValue)/100 ELSE ConcessionValue END
                                        FROM V3M_FIN_DetMapStudentFeeHeadConcession
                                        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND StudentId = @StudentId
                                          AND ClassCode = @ClassCode AND IsValid = 1 AND FeeHeadId = @FeeHeadId
                                    END
                                    ELSE
                                    BEGIN
                                        SELECT @ConcessionAmount = CASE ConcessionType WHEN 0 THEN (@FeeHeadAmount * ConcessionApplicable)/100 ELSE ConcessionApplicable END
                                        FROM V3M_FIN_DetCocessionMapToFeeHead
                                        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId
                                          AND ConcessionId = @ConcessionId AND FeeHeadId = @FeeHeadId AND IsValid = 1
                                    END

                                    SET @IsRoundOff = 0
                                    IF EXISTS(SELECT KeyValue FROM DefaultSettings WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND KeyName='ConcessionRoundOff')
                                    BEGIN
                                        SELECT @IsRoundOff = KeyValue FROM DefaultSettings WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND KeyName='ConcessionRoundOff'
                                        IF (@IsRoundOff = 1) SET @ConcessionAmount = ROUND(@ConcessionAmount, 0)
                                        ELSE IF (@IsRoundOff = 2) SET @ConcessionAmount = FLOOR(@ConcessionAmount)
                                        ELSE IF (@IsRoundOff = 3) SET @ConcessionAmount = CEILING(@ConcessionAmount)
                                    END

                                    IF @ConcessionAmount > 0
                                        SET @BalanceConcession = @ConcessionAmount - @ChallanConcessionAmount

                                    IF (@ConcessionAmount > @FeeHeadAmount)
                                    BEGIN
                                        INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
                                        VALUES('Finance','ViewStudentChallan.aspx','Proc V3M_FIN_UspApplyStudentConcessionOnChallan',
                                            'In Apply Concession amt can not be > Fee Head Amt. For InvoiceId=' + CAST(@InvoiceId AS varchar) + ', ConcessionId=' + CAST(@ConcessionId AS varchar), GETDATE())

                                        SET @ResultValue = -11
                                    END
                                    ELSE
                                    BEGIN
                                        IF @BalanceConcession <> 0
                                        BEGIN
                                            UPDATE V3M_FIN_DetInvoice SET Concession = Concession + @BalanceConcession
                                            WHERE InvoiceId=@InvoiceId AND FeeHeadId = @FeeheadId AND IsValid = 1 AND MonthNo = @MonthNo

                                            INSERT INTO V3M_FIN_FeeHeadLedger(GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType,
                                                Narration, IsValid, InvoiceId, TransDate, TransBy, CrDr, EnteredVia, SessionId,CreatedDate)
                                            VALUES(@GroupCode, @BranchCode, @SessionId, @FeeheadId, @MonthNo, @MonthNo, @SessionId, -@BalanceConcession, 'STUDENT', @StudentId, 'CON',
                                                'Concession (Id=' + CAST(@ConcessionId AS varchar) + ') applied by edit Challan.' + ISNULL(@Narration, ''), 1, @InvoiceId, GETDATE(), @CreatedBy, 'DR', 'Pro_V3M_FIN_UspApplyStudentConcessionOnChallan', @SessionId,GETDATE())

                                            SELECT @tmpHeadAmt=Amount, @tmpConcession=Concession, @tmpReceivedAmt=ReceivedAmt, @tmpWaiveOffAmt=WaiveOff
                                            FROM V3M_FIN_DetInvoice WHERE InvoiceId=@InvoiceId AND FeeHeadId = @FeeheadId AND IsValid = 1 AND MonthNo = @MonthNo

                                            SET @Advance = @tmpHeadAmt - (@tmpReceivedAmt + @tmpConcession + @tmpWaiveOffAmt)
                                            SET @TotalAdvanceAmount = @TotalAdvanceAmount + @Advance

                                            IF (@Advance < 0)
                                            BEGIN
                                                CLOSE CurrFeeMonth
                                                DEALLOCATE CurrFeeMonth
                                                CLOSE ConcessionMapCursor
                                                DEALLOCATE ConcessionMapCursor

                                                ROLLBACK TRAN

                                                INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
                                                VALUES('Finance','ViewStudentChallan.aspx','Proc V3M_FIN_UspApplyStudentConcessionOnChallan','In Concession received amount can not be < 0.', GETDATE())

                                                SET @ResultValue = -1
                                                SET @ResultMessage = 'Received amount cannot go below 0.'
                                                RETURN
                                            END
                                        END
                                        SET @TotalConcessionAmount = @TotalConcessionAmount + @BalanceConcession
                                    END
                                END

                                FETCH NEXT FROM CurrFeeMonth INTO @FeeHeadId, @MonthNo, @FeeHeadAmount, @ChallanConcessionAmount, @ChallanWaiveOffAmount, @MonthEndDate
                            END

                            CLOSE CurrFeeMonth
                            DEALLOCATE CurrFeeMonth

                            --=================================================================
                            -- Naya: is concession ka applied-amount, id, aur readable message banao
                            --=================================================================
                            SET @ThisConcessionApplied = @TotalConcessionAmount - @ConcessionAmountBeforeThis

                            IF @ThisConcessionApplied > 0
                            BEGIN
                                SET @AppliedConcessionIds = ISNULL(@AppliedConcessionIds + ',', '') + CAST(@ConcessionId AS varchar)
                                SET @ResultMessage = ISNULL(@ResultMessage + ' ', '') +
                                    ISNULL(@ConcessionDisplayName, 'ConcessionId ' + CAST(@ConcessionId AS varchar)) +
                                    ' Concession applied successfully (Amount: ' + CAST(@ThisConcessionApplied AS varchar) + ').'
                            END
                            ELSE
                            BEGIN
                                SET @SkippedConcessionIds = ISNULL(@SkippedConcessionIds + ',', '') + CAST(@ConcessionId AS varchar)
                                SET @ResultMessage = ISNULL(@ResultMessage + ' ', '') +
                                    ISNULL(@ConcessionDisplayName, 'ConcessionId ' + CAST(@ConcessionId AS varchar)) +
                                    ' Concession could not be applied (not valid for any fee month on this invoice).'
                            END

                            FETCH NEXT FROM ConcessionMapCursor INTO @ConcessionId, @ConcessionFromDate, @ConcessionToDate
                        END

                        CLOSE ConcessionMapCursor
                        DEALLOCATE ConcessionMapCursor

                        IF @TotalConcessionAmount <> 0
                        BEGIN
                            UPDATE V3M_FIN_MstInvoice SET TotalConcession = TotalConcession + @TotalConcessionAmount
                            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND InvoiceId = @InvoiceId

                            INSERT INTO V3M_FIN_StudentLedger(GroupCode, BranchCode, SessionId, StudentId, ReferenceNo, ReferenceType, Narration, Amount,
                                TransMode, TransDetail, RunningBalance, TransactionDate, TransactionBy,CreatedDate)
                            VALUES(@GroupCode, @BranchCode, @SessionId, @StudentId, @InvoiceNo, 'CHALLAN',
                                'Concession (Id=' + CAST(@ConcessionId AS varchar) + ') applied by edit Challan.' +CASE WHEN @Narration IS NOT NULL AND LTRIM(RTRIM(@Narration)) <> '' THEN ' ' + @Narration ELSE '' END,
                                -@TotalConcessionAmount, '', '', dbo.Fn_GetStudentRunningBalance(@GroupCode, @BranchCode, @SessionId, @StudentId, -@TotalConcessionAmount), GETDATE(), @CreatedBy,Getdate())

                            IF (@TotalAdvanceAmount < 0)
                            BEGIN
                                IF EXISTS(SELECT * FROM v3m_fin_Studentbalance WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND StudentId=@StudentId AND SessionId=@SessionId AND IsValid=1)
                                    UPDATE v3m_fin_Studentbalance SET Balance = Balance + @TotalAdvanceAmount
                                    WHERE GroupCode=@GroupCode AND BranchCode=@BranchCode AND StudentId=@StudentId AND SessionId=@SessionId AND IsValid=1
                                ELSE
                                    INSERT INTO v3m_fin_Studentbalance(GroupCode,BranchCode,StudentId,Balance,CreatedDate,CreatedBy,SessionId,IsValid)
                                    VALUES(@GroupCode,@BranchCode,@StudentId,@TotalAdvanceAmount,GETDATE(),@CreatedBy,@SessionId,1)
                            END

                            SELECT @tmpchBalance = (TotalAmount+ArrearAmount) - (TotalConcession+ReceivedAmount)
                            FROM V3M_FIN_MstInvoice WHERE InvoiceId=@InvoiceId

                            IF (@tmpchBalance <= 0)
                                UPDATE V3M_FIN_MstInvoice SET Ch_Status=3 WHERE InvoiceId=@InvoiceId
                            ELSE
                                UPDATE V3M_FIN_MstInvoice SET Ch_Status=2 WHERE InvoiceId=@InvoiceId

                            SET @ResultValue = 10
                        END
                        ELSE
                            SET @ResultValue = 0
                    END
                END
                ELSE
                BEGIN
                    SET @ResultValue = 6
                    SET @ResultMessage = 'No active concession mapping found for this student.'
                END
            END
        END
    END

  COMMIT TRAN
 END TRY
 BEGIN CATCH
    IF CURSOR_STATUS('local','CurrFeeMonth') >= -1
    BEGIN
        CLOSE CurrFeeMonth
        DEALLOCATE CurrFeeMonth
    END
    IF CURSOR_STATUS('local','ConcessionMapCursor') >= -1
    BEGIN
        CLOSE ConcessionMapCursor
        DEALLOCATE ConcessionMapCursor
    END

    IF XACT_STATE() <> 0
        ROLLBACK TRAN

    INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
    VALUES('Finance','ViewStudentChallan.aspx','Proc V3M_FIN_UspApplyStudentConcessionOnChallan',ERROR_MESSAGE(),GETDATE())

    SET @ResultValue = -1
    SET @ResultMessage = ERROR_MESSAGE()
 END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_UspApplyStudentConcessionOnChallan - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

-- V3M_FIN_UspRecalculateChallan

ALTER PROC [dbo].[V3M_FIN_UspRecalculateChallan]
    @GroupCode varchar(20), @BranchCode varchar(20), @SessionId bigint, @InvoiceId bigint,
    @UpdatedBy varchar(50), @ReturnValue varchar(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON
    SET @ReturnValue = '-1'

    DECLARE @StudentId bigint, @ClassCode varchar(50), @PeriodId int, @InvoiceTypeId int, @ActualReceived decimal(18,2)

    SELECT @StudentId = StudentId, @ClassCode = ClassCode, @PeriodId = PeriodId,
           @InvoiceTypeId = InvType, @ActualReceived = ISNULL(ActualReceived,0)
    FROM V3M_FIN_MstInvoice
    WHERE InvoiceId = @InvoiceId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId

    IF @StudentId IS NULL
    BEGIN
        SET @ReturnValue = '0'   -- invoice not found
        RETURN
    END

    IF @ActualReceived > 0
    BEGIN
        SET @ReturnValue = '2'   -- already has payment(s) - cannot safely recalculate
        RETURN
    END

    BEGIN TRY
        BEGIN TRANSACTION

        CREATE TABLE #tblRecalc(FeeHeadId int, HeadName varchar(50), HeadAmount decimal(18,2), ConcessionAmount decimal(18,2),
            AllMonthsDisplay varchar(100), AllMonths varchar(50), TotalConcession decimal(18,2), TotalAmount decimal(18,2),
            DisplayOrder int, HeadType varchar(50), FeeType varchar(50), NoMonth INT DEFAULT 0)

        INSERT INTO #tblRecalc
        EXEC V3M_FIN_UspGetFeeDetailsForInvoiceWithTemplate @GroupCode, @BranchCode, @SessionId, @ClassCode, @StudentId, @PeriodId, @InvoiceTypeId

        DECLARE @NewTotal decimal(18,2), @NewConcession decimal(18,2)
        SELECT @NewTotal = ISNULL(SUM(TotalAmount),0), @NewConcession = ISNULL(SUM(TotalConcession),0) FROM #tblRecalc

        -- Invalidate this invoice's prior DUE/CON ledger lines rather than deleting them (audit trail intact)
        UPDATE V3M_FIN_FeeHeadLedger SET IsValid = 0
        WHERE InvoiceId = @InvoiceId AND TransType IN ('DUE','CON')

        DELETE FROM V3M_FIN_DetInvoice WHERE InvoiceId = @InvoiceId

        DECLARE @CH_FeeHeadId int, @CH_HeadAmount decimal(18,2), @AllMonths int, @DisplayOrder int, @ConcessionAmt decimal(18,2)
        DECLARE CURR_RECALC CURSOR LOCAL FAST_FORWARD FOR
            SELECT FeeHeadId, HeadAmount, AllMonths, DisplayOrder, TotalConcession FROM #tblRecalc
        OPEN CURR_RECALC
        FETCH NEXT FROM CURR_RECALC INTO @CH_FeeHeadId, @CH_HeadAmount, @AllMonths, @DisplayOrder, @ConcessionAmt
        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @DetInvoiceId bigint
            SELECT @DetInvoiceId = ISNULL(MAX(DetInvoiceId),0) + 1 FROM V3M_FIN_DetInvoice

            INSERT INTO V3M_FIN_DetInvoice(DetInvoiceId, InvoiceId, FeeHeadId, DisplayOrder, MonthNo, Amount, Concession, ReceivedAmt, Adjusted, WaiveOff, IsValid, Status)
            VALUES(@DetInvoiceId, @InvoiceId, @CH_FeeHeadId, @DisplayOrder, @AllMonths, @CH_HeadAmount, @ConcessionAmt, 0, 0, 0, 1,
                   CASE WHEN @CH_HeadAmount = @ConcessionAmt THEN 3 ELSE 1 END)

            INSERT INTO V3M_FIN_FeeHeadLedger(GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor,
                Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, CRDR, EnteredVia)
            VALUES(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(GETDATE()), @SessionId, @CH_HeadAmount,
                'STUDENT', @StudentId, 'DUE', 'Recalculated - Invoice #' + CAST(@InvoiceId AS varchar(20)), 1, @InvoiceId,
                GETDATE(), @UpdatedBy, 'DR', 'Pro_V3M_FIN_UspRecalculateChallan')

            IF @ConcessionAmt > 0
                INSERT INTO V3M_FIN_FeeHeadLedger(GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor,
                    Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, CRDR, EnteredVia)
                VALUES(@GroupCode, @BranchCode, '1', @CH_FeeHeadId, @AllMonths, MONTH(GETDATE()), @SessionId, -@ConcessionAmt,
                    'STUDENT', @StudentId, 'CON', 'Recalculated - Invoice #' + CAST(@InvoiceId AS varchar(20)), 1, @InvoiceId,
                    GETDATE(), @UpdatedBy, 'DR', 'Pro_V3M_FIN_UspRecalculateChallan')

            FETCH NEXT FROM CURR_RECALC INTO @CH_FeeHeadId, @CH_HeadAmount, @AllMonths, @DisplayOrder, @ConcessionAmt
        END
        CLOSE CURR_RECALC
        DEALLOCATE CURR_RECALC

        UPDATE V3M_FIN_MstInvoice SET TotalAmount = @NewTotal, TotalConcession = @NewConcession WHERE InvoiceId = @InvoiceId

        DROP TABLE #tblRecalc
        SET @ReturnValue = '1'
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
        INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES('Finance','RecalculateChallan','V3M_FIN_UspRecalculateChallan', ERROR_MESSAGE(), GETDATE())
        SET @ReturnValue = '-1'
    END CATCH
END

GO
IF @@ERROR <> 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT = 0 BEGIN PRINT 'FAILED at V3M_FIN_UspRecalculateChallan - all changes rolled back. Nothing was altered.'; SET NOEXEC ON; END
GO

IF @@TRANCOUNT > 0 BEGIN COMMIT TRANSACTION; PRINT 'Rollback complete.'; END
GO
SET NOEXEC OFF;
GO
