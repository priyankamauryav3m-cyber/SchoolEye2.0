/*
    Admission Fee Collection — month wise fee particulars
    ----------------------------------------------------------------
    V3M_FIN_GetClassMapTempateFeeHead now also returns, per fee head:
      MonthNames    : months of the period with an amount > 0, session order (e.g. "Apr, May, Jun")
      MonthlyAmount : amount of one month (NULL when those months have different amounts)
    Amount stays the total of the period (unchanged), so existing callers keep working.

    Caller  : AdmitChildService.GetClassMapTemplateFeeHead (only caller)
    Safety  : ALTER of a read-only procedure, two extra columns. No data is changed.
    Rollback: see bottom (commented out) — the previous definition.
*/

SET NOCOUNT ON;
GO

ALTER PROCEDURE dbo.V3M_FIN_GetClassMapTempateFeeHead
(
    @GroupCode VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ClassCode VARCHAR(20),
    @SessionId INT,
    @PeriodId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- one row per fee head per month of the period
    WITH MonthWise AS
    (
        SELECT CDFT.ClassCode, FCC.FeeTemplateId, FCC.FeeHeadId, FH.FeeHeadName, FH.FeeHeadType, PFC.PeriodId,
            FCC.SessionId, FCC.MonthNo, SUM(FCC.Amount) AS MonthAmount
        FROM V3M_FIN_ClassDefaultFeeTemplate CDFT
        INNER JOIN V3M_FIN_Det_FeeCollectionConfig FCC
            ON FCC.GroupCode = CDFT.GroupCode AND FCC.BranchCode = CDFT.BranchCode
            AND FCC.FeeTemplateId = CDFT.DefaultFeeTemplateId  AND FCC.SessionId = CDFT.SessionId
        INNER JOIN V3M_FIN_Det_FeeCollectionPeriodConfig PFC
            ON PFC.MonthNo = FCC.MonthNo AND PFC.IsValid = 1
        LEFT JOIN V3M_FIN_MstFeeHead FH
            ON FH.FeeHeadId = FCC.FeeHeadId
        WHERE
            CDFT.GroupCode = @GroupCode
            AND CDFT.BranchCode = @BranchCode  AND CDFT.ClassCode = @ClassCode
            AND CDFT.SessionId = @SessionId   AND CDFT.IsValid = 1 AND FCC.IsValid = 1
            AND PFC.PeriodId = @PeriodId
        GROUP BY CDFT.ClassCode, FCC.FeeTemplateId, FCC.FeeHeadId, FH.FeeHeadName, FH.FeeHeadType, PFC.PeriodId,
            FCC.SessionId, FCC.MonthNo
    )
    SELECT ClassCode, FeeTemplateId, FeeHeadId, FeeHeadName, FeeHeadType, PeriodId,
        SUM(MonthAmount) AS Amount,
        -- only months that carry an amount (a one-time fee is configured as 0 in the other months)
        CASE
            WHEN MAX(MonthAmount) <= 0 THEN 0
            WHEN MIN(CASE WHEN MonthAmount > 0 THEN MonthAmount END) = MAX(MonthAmount) THEN MAX(MonthAmount)
        END AS MonthlyAmount,
        -- session order: Apr .. Mar
        STRING_AGG(CASE WHEN MonthAmount > 0 THEN LEFT(DATENAME(MONTH, DATEFROMPARTS(2000, MonthNo, 1)), 3) END, ', ')
            WITHIN GROUP (ORDER BY (MonthNo + 8) % 12) AS MonthNames,
        SessionId
    FROM MonthWise
    GROUP BY ClassCode, FeeTemplateId, FeeHeadId, FeeHeadName, FeeHeadType, PeriodId, SessionId
    ORDER BY FeeHeadId;
END;
GO

/* ---------------- Rollback ----------------
ALTER PROCEDURE dbo.V3M_FIN_GetClassMapTempateFeeHead
(
    @GroupCode VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ClassCode VARCHAR(20),
    @SessionId INT,
    @PeriodId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CDFT.ClassCode, FCC.FeeTemplateId, FCC.FeeHeadId, FH.FeeHeadName, FH.FeeHeadType, PFC.PeriodId, SUM(FCC.Amount) AS Amount,
        FCC.SessionId
    FROM V3M_FIN_ClassDefaultFeeTemplate CDFT
    INNER JOIN V3M_FIN_Det_FeeCollectionConfig FCC
        ON FCC.GroupCode = CDFT.GroupCode AND FCC.BranchCode = CDFT.BranchCode
        AND FCC.FeeTemplateId = CDFT.DefaultFeeTemplateId  AND FCC.SessionId = CDFT.SessionId
    INNER JOIN V3M_FIN_Det_FeeCollectionPeriodConfig PFC
        ON PFC.MonthNo = FCC.MonthNo AND PFC.IsValid = 1
    LEFT JOIN V3M_FIN_MstFeeHead FH
        ON FH.FeeHeadId = FCC.FeeHeadId
    WHERE
        CDFT.GroupCode = @GroupCode
        AND CDFT.BranchCode = @BranchCode  AND CDFT.ClassCode = @ClassCode
        AND CDFT.SessionId = @SessionId   AND CDFT.IsValid = 1 AND FCC.IsValid = 1
        AND PFC.PeriodId = @PeriodId
       GROUP BY CDFT.ClassCode, FCC.FeeTemplateId, FCC.FeeHeadId, FH.FeeHeadName, FH.FeeHeadType, PFC.PeriodId, FCC.SessionId
       ORDER BY FCC.FeeHeadId;
END;
GO
*/
