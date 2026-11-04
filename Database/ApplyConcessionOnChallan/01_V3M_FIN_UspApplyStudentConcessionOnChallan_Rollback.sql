/*
    Rollback: puts back V3M_FIN_UspApplyStudentConcessionOnChallan exactly as deployed on live (modified 21-10-2026).
    Removes the 04-11-2026 fix (concession picked its own fee heads / own validity).
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
