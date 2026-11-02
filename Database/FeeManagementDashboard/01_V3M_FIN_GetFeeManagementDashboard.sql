/*
    V3M_FIN_GetFeeManagementDashboard  (02-11-2026)
    Read-only. Feeds the Fee Management dashboard page (/FeeManagement).

    Returns ONE row:
      Counts      : TotalStudents, TransportUsers, TotalStaff, EwsStudents
      Session     : TotalFeeDue, TotalConcession, TotalWaiveOff, LateFine, TotalReceived, TotalOutstanding,
                    PreSessionAdvance, CurrentAdvance
                    (same invoice rule as Fn_GetStudentBalance: Ch_Status > 0)
      Period      : PeriodCollected, PeriodCash, PeriodCheque, PeriodOnline, PeriodOther, PeriodReceiptCount
                    (valid receipts with receipt date between @FromDate and @ToDate)
      Expenses    : TotalExpenses, TodayExpenses -> 0, there is no expense module/table yet.

    Advances are returned as positive amounts (V3M_Fin_StudentBalance keeps them negative).

    Rollback: DROP PROCEDURE dbo.V3M_FIN_GetFeeManagementDashboard;   (new object, nothing else uses it)

    Test:
      EXEC dbo.V3M_FIN_GetFeeManagementDashboard '01','01',1043,'2026-11-02','2026-11-02';
*/
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetFeeManagementDashboard
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @SessionId  BIGINT,
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @FromDate = ISNULL(@FromDate, CAST(GETDATE() AS DATE));
    SET @ToDate   = ISNULL(@ToDate, @FromDate);
    IF @ToDate < @FromDate
    BEGIN
        DECLARE @Swap DATE = @FromDate;
        SET @FromDate = @ToDate;
        SET @ToDate = @Swap;
    END;

    DECLARE @ToDateExclusive DATE = DATEADD(DAY, 1, @ToDate);

    /* ---------------- Student / staff counts ---------------- */
    DECLARE @TotalStudents INT = 0, @TransportUsers INT = 0, @EwsStudents INT = 0, @TotalStaff INT = 0;

    SELECT
        @TotalStudents  = COUNT(DISTINCT sj.StudentId),
        @TransportUsers = COUNT(DISTINCT CASE WHEN ISNULL(sj.DistanceId, 0) > 0 OR sj.IsTPTRequired = 1
                                              THEN sj.StudentId END),
        @EwsStudents    = COUNT(DISTINCT CASE WHEN ISNULL(sm.EWSId, 0) > 0 THEN sj.StudentId END)
    FROM dbo.StudentJourney sj
    INNER JOIN dbo.StudentMaster sm
            ON sm.StudentId = sj.StudentId
           AND sm.IsValid = 1
           AND sm.WithdrawalDate IS NULL
    WHERE sj.GroupCode  = @GroupCode
      AND sj.BranchCode = @BranchCode
      AND sj.SessionId  = @SessionId
      AND sj.IsValid    = 1;

    SELECT @TotalStaff = COUNT(1)
    FROM dbo.MstEmployee e
    WHERE e.GroupCode  = @GroupCode
      AND e.BranchCode = @BranchCode
      AND e.IsValid    = 1
      AND e.LeaveDate IS NULL;

    /* ---------------- Session fee & balances ---------------- */
    DECLARE @TotalFeeDue DECIMAL(18,2) = 0, @TotalConcession DECIMAL(18,2) = 0,
            @TotalWaiveOff DECIMAL(18,2) = 0, @TotalReceived DECIMAL(18,2) = 0;

    SELECT
        @TotalFeeDue     = ISNULL(SUM(mi.TotalAmount), 0),
        @TotalConcession = ISNULL(SUM(mi.TotalConcession), 0),
        @TotalWaiveOff   = ISNULL(SUM(ISNULL(mi.TotalWaiveOff, 0)), 0),
        @TotalReceived   = ISNULL(SUM(mi.ReceivedAmount), 0)
    FROM dbo.V3M_FIN_MstInvoice mi
    WHERE mi.GroupCode  = @GroupCode
      AND mi.BranchCode = @BranchCode
      AND mi.SessionId  = @SessionId
      AND mi.Ch_Status  > 0;

    DECLARE @LateFine DECIMAL(18,2) = 0;
    SELECT @LateFine = ISNULL(SUM(rm.LateFee), 0)
    FROM dbo.V3M_FIN_StudentReceiptMaster rm
    WHERE rm.GroupCode  = @GroupCode
      AND rm.BranchCode = @BranchCode
      AND rm.SessionId  = @SessionId
      AND rm.Valid      = 1;

    DECLARE @PreSessionAdvance DECIMAL(18,2) = 0, @CurrentAdvance DECIMAL(18,2) = 0;
    SELECT
        @PreSessionAdvance = ISNULL(SUM(CASE WHEN sb.IsCarryFwd = 1 THEN -sb.Balance END), 0),
        @CurrentAdvance    = ISNULL(SUM(CASE WHEN ISNULL(sb.IsCarryFwd, 0) = 0 THEN -sb.Balance END), 0)
    FROM dbo.V3M_Fin_StudentBalance sb
    WHERE sb.GroupCode  = @GroupCode
      AND sb.BranchCode = @BranchCode
      AND sb.SessionId  = @SessionId
      AND sb.IsValid    = 1
      AND sb.Balance    < 0;

    /* ---------------- Collection in the selected period (by payment mode) ---------------- */
    DECLARE @PeriodCash DECIMAL(18,2) = 0, @PeriodCheque DECIMAL(18,2) = 0,
            @PeriodOnline DECIMAL(18,2) = 0, @PeriodOther DECIMAL(18,2) = 0, @PeriodReceiptCount INT = 0;

    SELECT
        @PeriodCash   = ISNULL(SUM(CASE WHEN m.Mode = 'CASH' THEN m.PaidAmount END), 0),
        @PeriodCheque = ISNULL(SUM(CASE WHEN m.Mode IN ('CHEQUE', 'DD', 'DEMAND DRAFT') THEN m.PaidAmount END), 0),
        @PeriodOnline = ISNULL(SUM(CASE WHEN m.Mode IN ('ONLINE', 'UPI', 'NEFT', 'RTGS', 'IMPS', 'CARD', 'DEBIT CARD',
                                                         'CREDIT CARD', 'NET BANKING', 'BANK TRANSFER', 'POS') THEN m.PaidAmount END), 0),
        @PeriodOther  = ISNULL(SUM(CASE WHEN m.Mode NOT IN ('CASH', 'CHEQUE', 'DD', 'DEMAND DRAFT', 'ONLINE', 'UPI', 'NEFT', 'RTGS',
                                                             'IMPS', 'CARD', 'DEBIT CARD', 'CREDIT CARD', 'NET BANKING',
                                                             'BANK TRANSFER', 'POS') THEN m.PaidAmount END), 0),
        @PeriodReceiptCount = COUNT(1)
    FROM
    (
        SELECT UPPER(LTRIM(RTRIM(ISNULL(rm.PaymentMode, '')))) AS Mode,
               ISNULL(rm.PaidAmount, 0) AS PaidAmount
        FROM dbo.V3M_FIN_StudentReceiptMaster rm
        WHERE rm.GroupCode   = @GroupCode
          AND rm.BranchCode  = @BranchCode
          AND rm.SessionId   = @SessionId
          AND rm.Valid       = 1
          AND rm.CreatedDate >= @FromDate
          AND rm.CreatedDate <  @ToDateExclusive
    ) m;

    /* ---------------- Final row ---------------- */
    SELECT
        @TotalStudents      AS TotalStudents,
        @TransportUsers     AS TransportUsers,
        @TotalStaff         AS TotalStaff,
        @EwsStudents        AS EwsStudents,

        @TotalFeeDue        AS TotalFeeDue,
        @TotalConcession    AS TotalConcession,
        @TotalWaiveOff      AS TotalWaiveOff,
        @LateFine           AS LateFine,
        @TotalReceived      AS TotalReceived,
        CASE WHEN @TotalFeeDue - @TotalConcession - @TotalWaiveOff - @TotalReceived > 0
             THEN @TotalFeeDue - @TotalConcession - @TotalWaiveOff - @TotalReceived
             ELSE 0 END     AS TotalOutstanding,
        @PreSessionAdvance  AS PreSessionAdvance,
        @CurrentAdvance     AS CurrentAdvance,

        @PeriodCash + @PeriodCheque + @PeriodOnline + @PeriodOther AS PeriodCollected,
        @PeriodCash         AS PeriodCash,
        @PeriodCheque       AS PeriodCheque,
        @PeriodOnline       AS PeriodOnline,
        @PeriodOther        AS PeriodOther,
        @PeriodReceiptCount AS PeriodReceiptCount,

        CAST(0 AS DECIMAL(18,2)) AS TotalExpenses,
        CAST(0 AS DECIMAL(18,2)) AS TodayExpenses,

        @FromDate           AS FromDate,
        @ToDate             AS ToDate;
END
GO
