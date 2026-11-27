/* ============================================================================
   Summer Activity - Registrations tab (/SummerActivity, tab 4)

   1) Usp_GetSummerActivityRegistrationList
      every submitted registration (OtherActivities) of a group / branch / session
      @ActivityId  0 = all activities
      @Valid       1 active, 0 deactive, -1 all
      @PaymentMode '' = all, 'NOTPAID' = fee not paid, otherwise the saved payment mode (UPI, ...)

   2) Usp_GetSummerActivityRegistrationDetail
      filled form of one registration: one row per active field of its activity
      (columns are only the DynamicFormController field names; QUOTENAME + sp_executesql)

   3) Usp_GetSummerActivityPaymentModes
      payment modes saved in the branch (filter dropdown)

   4) Usp_ActiveDeactiveSummerActivityRegistration
      @Result 1 changed, -1 not found

   Rollback: DROP PROCEDURE of the four procedures.
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.Usp_GetSummerActivityRegistrationList
    @GroupCode   VARCHAR(50),
    @BranchCode  VARCHAR(50),
    @SessionId   BIGINT,
    @ActivityId  INT = 0,
    @Valid       INT = -1,
    @PaymentMode VARCHAR(50) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SELECT oa.OAId AS RegistrationId,
           sa.SAId,
           sa.ActivityName,
           oa.SlotName,
           ISNULL(oa.Amount, 0)                  AS Amount,
           CAST(ISNULL(oa.IsFeePaid, 0) AS BIT)  AS IsFeePaid,
           oa.PaymentMode,
           oa.RefNo                              AS OrderId,
           oa.BankTransacId                      AS TransactionId,
           oa.UTRNo                              AS UtrNo,
           oa.TransacDate                        AS PaidOn,
           oa.CreatedDate                        AS RegisteredOn,
           CAST(ISNULL(oa.IsValid, 1) AS BIT)    AS IsValid
    FROM OtherActivities oa
    INNER JOIN SummerActivities sa
            ON sa.SAId = oa.Activity AND sa.GroupCode = oa.GroupCode AND sa.BranchCode = oa.BranchCode
    WHERE oa.GroupCode = @GroupCode
      AND oa.BranchCode = @BranchCode
      AND sa.SessionId = @SessionId
      AND (@ActivityId = 0 OR oa.Activity = @ActivityId)
      AND (@Valid = -1 OR ISNULL(oa.IsValid, 1) = @Valid)
      AND (   ISNULL(@PaymentMode, '') = ''
           OR (@PaymentMode = 'NOTPAID' AND ISNULL(oa.IsFeePaid, 0) = 0)
           OR (ISNULL(oa.IsFeePaid, 0) = 1 AND oa.PaymentMode = @PaymentMode))
    ORDER BY oa.OAId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.Usp_GetSummerActivityRegistrationDetail
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @OAId       INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SAId INT;
    SELECT @SAId = Activity FROM OtherActivities
    WHERE OAId = @OAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;
    IF @SAId IS NULL
        RETURN;

    -- one VALUES row per active field that is a column of OtherActivities
    DECLARE @Rows NVARCHAR(MAX);
    SELECT @Rows = STRING_AGG(CAST(
               N'(' + CAST(d.DFCId AS NVARCHAR(12)) + N', '
               + CASE WHEN TYPE_NAME(c.system_type_id) IN ('datetime', 'datetime2', 'date', 'smalldatetime')
                      THEN N'CONVERT(NVARCHAR(10), oa.' + QUOTENAME(c.name) + N', 103)'
                      ELSE N'CONVERT(NVARCHAR(MAX), oa.' + QUOTENAME(c.name) + N')' END
               + N')' AS NVARCHAR(MAX)), N', ')
    FROM DynamicFormController d
    INNER JOIN sys.columns c
            ON c.object_id = OBJECT_ID(N'dbo.OtherActivities')
           AND c.name COLLATE DATABASE_DEFAULT = d.FieldName COLLATE DATABASE_DEFAULT
    WHERE d.GroupCode = @GroupCode AND d.BranchCode = @BranchCode AND d.SAId = @SAId AND d.IsValid = 1;

    IF @Rows IS NULL
        RETURN;

    DECLARE @SQL NVARCHAR(MAX) = N'
        SELECT d.FieldName, d.DisplayName, d.FieldType, v.Value
        FROM dbo.OtherActivities oa
        CROSS APPLY (VALUES ' + @Rows + N') v(DFCId, Value)
        INNER JOIN dbo.DynamicFormController d ON d.DFCId = v.DFCId
        WHERE oa.OAId = @OAId
        ORDER BY d.DisplayOrder, d.DFCId;';
    EXEC sp_executesql @SQL, N'@OAId INT', @OAId = @OAId;
END
GO

CREATE OR ALTER PROCEDURE dbo.Usp_GetSummerActivityPaymentModes
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT PaymentMode
    FROM OtherActivities
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode
      AND ISNULL(IsFeePaid, 0) = 1 AND ISNULL(PaymentMode, '') <> ''
    ORDER BY PaymentMode;
END
GO

CREATE OR ALTER PROCEDURE dbo.Usp_ActiveDeactiveSummerActivityRegistration
    @GroupCode  VARCHAR(50),
    @BranchCode VARCHAR(50),
    @OAId       INT,
    @IsValid    BIT,
    @Result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE OtherActivities
       SET IsValid = @IsValid
     WHERE OAId = @OAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

    SET @Result = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE -1 END;
END
GO
