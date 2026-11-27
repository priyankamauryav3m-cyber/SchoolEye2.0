/* ============================================================================
   Summer activity online payment (checkout page /summer-activity-checkout)

   1) Usp_InitiateSummerActivityPayment
      Proceed to Pay: one pending row in V3M_FIN_Online_SummerActivities_InitiateTransDetails
      (Status 0, PaymentStatus 0) with a new order id (RefNo); OtherActivities.RefNo = that order id.
      A pending order of the last @ValidMinutes is reused (refresh of the page does not make a new one).
      @Result 1 ok, -1 registration not found, -2 fee already paid, -3 no fee amount, -99 error

   2) Usp_CompleteSummerActivityPayment
      Success of the payment of the order: the transaction row -> Status 1 / PaymentStatus 1 / TransId,
      OtherActivities -> IsFeePaid 1, BankTransacId, RefNo, TransacDate, UTRNo, PaymentMode.
      For now it is called by the local TEST payment (PaymentGateway 'TEST'); the real gateway
      callback will call the same procedure after it verifies the gateway response.
      @Result 1 paid, -1 order not found, -2 already paid, -3 order expired, -99 error

   USP_GetOtherActivitiesData joins the transaction table on RefNo, so the report shows the gateway.
   Rollback: DROP PROCEDURE dbo.Usp_InitiateSummerActivityPayment; DROP PROCEDURE dbo.Usp_CompleteSummerActivityPayment;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_InitiateSummerActivityPayment
    @GroupCode      VARCHAR(50),
    @BranchCode     VARCHAR(50),
    @SessionId      BIGINT,
    @OAId           INT,
    @PaymentGateway VARCHAR(50),
    @ValidMinutes   INT = 10,
    @RefNo          NVARCHAR(100) OUTPUT,
    @SecondsLeft    INT OUTPUT,
    @Result         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RefNo = NULL;
    SET @SecondsLeft = 0;

    DECLARE @Amount DECIMAL(18, 2), @IsFeePaid BIT;
    SELECT @Amount = oa.Amount, @IsFeePaid = ISNULL(oa.IsFeePaid, 0)
    FROM OtherActivities oa
    INNER JOIN SummerActivities sa
            ON sa.SAId = oa.Activity AND sa.GroupCode = oa.GroupCode AND sa.BranchCode = oa.BranchCode
    WHERE oa.OAId = @OAId AND oa.GroupCode = @GroupCode AND oa.BranchCode = @BranchCode
      AND sa.SessionId = @SessionId AND ISNULL(oa.IsValid, 1) = 1;

    IF @@ROWCOUNT = 0 BEGIN SET @Result = -1; RETURN; END
    IF @IsFeePaid = 1 BEGIN SET @Result = -2; RETURN; END
    IF ISNULL(@Amount, 0) <= 0 BEGIN SET @Result = -3; RETURN; END

    BEGIN TRY
        BEGIN TRAN;

        -- pending order of this registration that is still valid
        DECLARE @Started DATETIME;
        SELECT TOP (1) @RefNo = t.RefNo, @Started = t.TransDate
        FROM V3M_FIN_Online_SummerActivities_InitiateTransDetails t WITH (UPDLOCK, HOLDLOCK)
        WHERE t.GroupCode = @GroupCode AND t.BranchCode = @BranchCode AND t.OAId = @OAId
          AND t.Status = 0 AND t.PaymentGateway = @PaymentGateway
          AND t.Amount = @Amount
          AND t.TransDate > DATEADD(MINUTE, -@ValidMinutes, GETDATE())
        ORDER BY t.SeedId DESC;

        IF @RefNo IS NULL
        BEGIN
            -- order id: SA + yyMMddHHmmss + registration no.
            SET @RefNo = N'SA' + FORMAT(GETDATE(), 'yyMMddHHmmss') + CAST(@OAId AS NVARCHAR(12));
            SET @Started = GETDATE();

            INSERT INTO V3M_FIN_Online_SummerActivities_InitiateTransDetails
                (GroupCode, BranchCode, RefNo, TransId, OAId, Amount, TransDate, PaymentGateway, ModeOfPayment,
                 PaymentStatus, TransType, Status, ProcessFee, ServiceTax, RegistrationId, SessionId)
            VALUES
                (@GroupCode, @BranchCode, @RefNo, NULL, @OAId, @Amount, @Started, @PaymentGateway, NULL,
                 0, 'SummerActivity', 0, 0, 0, @OAId, @SessionId);

            UPDATE OtherActivities SET RefNo = @RefNo
            WHERE OAId = @OAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;
        END

        COMMIT TRAN;
        SET @SecondsLeft = @ValidMinutes * 60 - DATEDIFF(SECOND, @Started, GETDATE());
        IF @SecondsLeft < 0 SET @SecondsLeft = 0;
        SET @Result = 1;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        SET @RefNo = NULL;
        SET @Result = -99;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.Usp_CompleteSummerActivityPayment
    @GroupCode      VARCHAR(50),
    @BranchCode     VARCHAR(50),
    @SessionId      BIGINT,
    @OAId           INT,
    @RefNo          NVARCHAR(100),
    @ModeOfPayment  VARCHAR(20),
    @TransId        NVARCHAR(200),
    @UTRNo          VARCHAR(50),
    @ValidMinutes   INT = 10,
    @Result         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @SeedId INT, @Status INT, @Started DATETIME;
        SELECT TOP (1) @SeedId = t.SeedId, @Status = ISNULL(t.Status, 0), @Started = t.TransDate
        FROM V3M_FIN_Online_SummerActivities_InitiateTransDetails t WITH (UPDLOCK, HOLDLOCK)
        WHERE t.GroupCode = @GroupCode AND t.BranchCode = @BranchCode AND t.SessionId = @SessionId
          AND t.OAId = @OAId AND t.RefNo = @RefNo
        ORDER BY t.SeedId DESC;

        IF @SeedId IS NULL BEGIN SET @Result = -1; COMMIT TRAN; RETURN; END

        IF @Status = 1 OR EXISTS (SELECT 1 FROM OtherActivities WITH (UPDLOCK)
                                  WHERE OAId = @OAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode
                                    AND ISNULL(IsFeePaid, 0) = 1)
        BEGIN SET @Result = -2; COMMIT TRAN; RETURN; END

        IF @Started <= DATEADD(MINUTE, -@ValidMinutes, GETDATE())
        BEGIN SET @Result = -3; COMMIT TRAN; RETURN; END

        UPDATE V3M_FIN_Online_SummerActivities_InitiateTransDetails
           SET TransId = @TransId, ModeOfPayment = @ModeOfPayment, PaymentStatus = 1, Status = 1, TransDate = GETDATE()
         WHERE SeedId = @SeedId;

        UPDATE OtherActivities
           SET IsFeePaid = 1, BankTransacId = LEFT(@TransId, 50), RefNo = LEFT(@RefNo, 50), TransacDate = GETDATE(),
               UTRNo = @UTRNo, PaymentMode = @ModeOfPayment
         WHERE OAId = @OAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        COMMIT TRAN;
        SET @Result = 1;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        SET @Result = -99;
    END CATCH
END
GO
