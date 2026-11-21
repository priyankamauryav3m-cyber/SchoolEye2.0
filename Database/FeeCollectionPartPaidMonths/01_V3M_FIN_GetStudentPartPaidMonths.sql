/*
    V3M_FIN_GetStudentPartPaidMonths  (01, 21-11-2026)  - partly paid months for Fee Collection (/feecollectionnew)

    Problem : Fee Collection lists only fee head lines that are still due
              (V3M_FIN_UspGetStudentFeeHeadDuesForAdjustmentDemo: ReceivedAmt < due). When an earlier receipt paid
              part of a month (April 28,000 due, 30,000 paid -> 2,000 went into May), the May line that was paid in
              full (e.g. a 1,000 head) disappears and the user only sees "Received 1,000" on the other head, so it is
              not visible that 2,000 was already received for May.
    Change  : new read-only procedure. For every month of the student that is partly paid (something received
              and something still due) it returns ALL fee head lines of that month, including the fully paid ones,
              with what was received on each. The screen shows a month summary from it
              (May: total 6,000, received 2,000, remaining 4,000).
              No table and no existing procedure is changed.
    Callers : StudentFeeService.GetStudentPartPaidMonths (API StudentFee/GetStudentPartPaidMonths).
    Rollback: 01_V3M_FIN_GetStudentPartPaidMonths_Rollback.sql (drops this procedure only).
*/
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetStudentPartPaidMonths
(
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @SessionId  BIGINT,
    @StudentId  BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Lines AS
    (
        SELECT
            dcn.FeeHeadId,
            mfh.FeeHeadName,
            dcn.MonthNo,
            CASE dcn.MonthNo WHEN 0 THEN ''
                 ELSE LEFT(DATENAME(MM, CAST('2015/' + CONVERT(VARCHAR, dcn.MonthNo) + '/1' AS DATETIME)), 3) END AS MonthDisplay,
            ISNULL(dcn.Amount, 0)      AS Amount,
            ISNULL(dcn.Concession, 0)  AS Concession,
            ISNULL(dcn.WaiveOff, 0)    AS WaiveOff,
            ISNULL(dcn.ReceivedAmt, 0) AS ReceivedAmt,
            ISNULL(dcn.Amount, 0) - ISNULL(dcn.Concession, 0) - ISNULL(dcn.WaiveOff, 0) AS Due,
            mfh.DisplayOrder AS HeadOrder,
            mdo.DisplayOrder AS MonthOrder
        FROM V3M_FIN_MstInvoice mcn
        INNER JOIN V3M_FIN_DetInvoice dcn ON mcn.InvoiceId = dcn.InvoiceId
        INNER JOIN V3M_FIN_MstFeeHead mfh ON dcn.FeeHeadId = mfh.FeeHeadId
        INNER JOIN V3M_FIN_Mst_MonthDisplayOrder mdo
                ON mcn.GroupCode = mdo.GroupCode AND mcn.BranchCode = mdo.BranchCode
               AND dcn.MonthNo = mdo.MonthNo AND mdo.IsValid = 1
        WHERE mcn.Ch_Status > 0
          AND mcn.GroupCode = @GroupCode AND mcn.BranchCode = @BranchCode
          AND mcn.SessionId = @SessionId AND mcn.StudentId = @StudentId
    ),
    PartPaidMonths AS
    (
        -- something received in the month and something still due
        SELECT MonthNo
        FROM Lines
        GROUP BY MonthNo
        HAVING SUM(ReceivedAmt) > 0
           AND SUM(CASE WHEN ReceivedAmt < Due THEN 1 ELSE 0 END) > 0
    )
    SELECT
        l.FeeHeadId,
        l.FeeHeadName,
        l.MonthNo,
        l.MonthDisplay,
        l.Amount,
        l.Concession,
        l.WaiveOff,
        l.ReceivedAmt,
        CAST(CASE WHEN l.ReceivedAmt >= l.Due THEN 1 ELSE 0 END AS BIT) AS IsFullyPaid
    FROM Lines l
    WHERE l.MonthNo IN (SELECT MonthNo FROM PartPaidMonths)
    ORDER BY l.MonthOrder, l.HeadOrder;
END
GO
