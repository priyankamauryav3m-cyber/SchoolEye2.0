/*
    Fix 04-11-2026: V3M_FIN_UspApplyStudentConcessionOnChallan (Edit Invoice -> Apply concession)

    Bug (student 130303, invoice 64):
      Staff Ward (Id=3) valid 04-Nov..31-Dec on head 12234, RTE (Id=2) valid 04-Nov..30-Nov on head 13236.
      Applying again put RTE's head 13236 on month 11 AND 12 with narration "Concession (Id=3)",
      i.e. RTE's fee head with Staff Ward's validity.

    Causes:
      1. 'Student' type amount was read from V3M_FIN_DetMapStudentFeeHeadConcession by StudentId + FeeHeadId only,
         so while processing one concession it picked up fee heads of another concession of the same student.
         -> now filtered by the mapping row (ConcStudId) being processed.
      2. If a concession was mapped again, the oldest active mapping (ConcStudId ASC) was used, with its old dates.
         -> now the latest active mapping of each concession is used.
      3. A concession that is already fully on the invoice was reported as "not valid for any fee month".
         -> now it is skipped with "already applied on this invoice", the other one is still applied.

    Callers: only API StudentFeeInvoice/ApplyStudentConcessionOnChallan (StudentInvoiceService.ApplyStudentConcessionOnChallanData).
    Parameters / output / ResultValue codes unchanged.
    Rollback: 01_V3M_FIN_UspApplyStudentConcessionOnChallan_Rollback.sql
*/
-- ---------------------------------------------------------------------------------------------------
-- V3M_FIN_UspApplyStudentConcessionOnChallan
-- ---------------------------------------------------------------------------------------------------
CREATE OR ALTER PROC [dbo].[V3M_FIN_UspApplyStudentConcessionOnChallan]
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
    DECLARE @ConcStudId bigint                      -- fix: jis mapping row ko process kar rahe hain
    DECLARE @ThisConcessionEligibleRows int = 0     -- fix: valid month + amount > 0 wali fee rows (already applied pehchanne ke liye)

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
                    -- fix: har concession ki sirf LATEST active mapping (apni ValidFrom / ValidUpto ke saath)
                    DECLARE ConcessionMapCursor CURSOR LOCAL FAST_FORWARD FOR
                        SELECT TOP 2 m.ConcStudId, m.ConcessionId, m.ValidFrom, m.ValidUpto
                        FROM (
                            SELECT dcs.ConcStudId, dcs.ConcessionId, dcs.ValidFrom, dcs.ValidUpto,
                                   ROW_NUMBER() OVER (PARTITION BY dcs.ConcessionId ORDER BY dcs.ConcStudId DESC) AS rn
                            FROM V3M_FIN_DetConcessionMapToStudent dcs
                            WHERE dcs.GroupCode = @GroupCode AND dcs.BranchCode = @BranchCode AND dcs.SessionId = @SessionId
                              AND dcs.ClassCode = @ClassCode AND dcs.Status = 1 AND dcs.StudentId = @StudentId
                              AND (
                                    @ConcessionIds IS NULL OR LTRIM(RTRIM(@ConcessionIds)) = ''
                                    OR dcs.ConcessionId IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@ConcessionIds, ',') WHERE LTRIM(RTRIM(value)) <> '')
                                  )
                        ) m
                        WHERE m.rn = 1
                        ORDER BY m.ConcStudId ASC

                    OPEN ConcessionMapCursor
                    FETCH NEXT FROM ConcessionMapCursor INTO @ConcStudId, @ConcessionId, @ConcessionFromDate, @ConcessionToDate

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
                            SET @ThisConcessionEligibleRows = 0

                            -- ?? naya: message ke liye concession ka display-naam nikalo
                            SET @ConcessionDisplayName = NULL
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
                                    SET @ConcessionName = NULL
                                    SELECT @ConcessionName = Type FROM V3M_FIN_MstConcession WHERE ConcessionId = @ConcessionId

                                    IF (@ConcessionName = 'Student')
                                    BEGIN
                                        -- fix: sirf isi mapping (ConcStudId) ke fee heads, dusre concession ke nahi
                                        SELECT @ConcessionAmount = CASE ConcessionType WHEN 0 THEN (@FeeHeadAmount * ConcessionValue)/100 ELSE ConcessionValue END
                                        FROM V3M_FIN_DetMapStudentFeeHeadConcession
                                        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SessionId = @SessionId AND StudentId = @StudentId
                                          AND ClassCode = @ClassCode AND IsValid = 1 AND FeeHeadId = @FeeHeadId
                                          AND ConcStudId = @ConcStudId
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
                                    BEGIN
                                        SET @BalanceConcession = @ConcessionAmount - @ChallanConcessionAmount
                                        SET @ThisConcessionEligibleRows = @ThisConcessionEligibleRows + 1
                                    END

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
                            ELSE IF @ThisConcessionEligibleRows > 0
                            BEGIN
                                -- fix: valid months par pehle se laga hua hai -> skip, dusra concession apply hota rahega
                                SET @SkippedConcessionIds = ISNULL(@SkippedConcessionIds + ',', '') + CAST(@ConcessionId AS varchar)
                                SET @ResultMessage = ISNULL(@ResultMessage + ' ', '') +
                                    ISNULL(@ConcessionDisplayName, 'ConcessionId ' + CAST(@ConcessionId AS varchar)) +
                                    ' Concession is already applied on this invoice (skipped).'
                            END
                            ELSE
                            BEGIN
                                SET @SkippedConcessionIds = ISNULL(@SkippedConcessionIds + ',', '') + CAST(@ConcessionId AS varchar)
                                SET @ResultMessage = ISNULL(@ResultMessage + ' ', '') +
                                    ISNULL(@ConcessionDisplayName, 'ConcessionId ' + CAST(@ConcessionId AS varchar)) +
                                    ' Concession could not be applied (not valid for any fee month on this invoice).'
                            END

                            FETCH NEXT FROM ConcessionMapCursor INTO @ConcStudId, @ConcessionId, @ConcessionFromDate, @ConcessionToDate
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

                            SELECT @tmpchBalance = TotalAmount - (TotalConcession+ReceivedAmount)
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
