/*
    RegistrationDemo — additive migration (demo/POC feature)
    ----------------------------------------------------------------
    Purpose : Demonstrates the Controller -> Service -> Repository -> Dapper -> SP -> SQL Server
              flow with a minimal entity. Named "RegistrationDemo" (not "Registration") so it does
              NOT collide with the existing production Registration feature/tables/procs.

    Safety  : Purely additive. Does not alter, drop, or touch any existing table/procedure/view.
              Safe to run multiple times (guarded with existence checks).

    Rollback: See the bottom of this file (commented out) — drops only the objects created here.
*/

SET NOCOUNT ON;
GO

-- ============================================================
-- 1. Table: ADM_RegistrationDemo
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ADM_RegistrationDemo')
BEGIN
    CREATE TABLE dbo.ADM_RegistrationDemo
    (
        RegiNo       INT IDENTITY(1,1) NOT NULL,   -- unique, auto-incremented
        GroupCode    VARCHAR(20)   NOT NULL,
        BranchCode   VARCHAR(20)   NOT NULL,
        SessionId    BIGINT        NOT NULL,
        StudentName  VARCHAR(150)  NOT NULL,
        CreatedBy    VARCHAR(50)   NULL,
        CreatedDate  DATETIME      NOT NULL CONSTRAINT DF_ADM_RegistrationDemo_CreatedDate DEFAULT (GETDATE()),
        IsValid      BIT           NOT NULL CONSTRAINT DF_ADM_RegistrationDemo_IsValid DEFAULT (1),

        CONSTRAINT PK_ADM_RegistrationDemo PRIMARY KEY CLUSTERED (RegiNo)
    );

    -- Supports the common lookup pattern (GroupCode/BranchCode/SessionId scoping) used across the app.
    CREATE NONCLUSTERED INDEX IX_ADM_RegistrationDemo_Tenant
        ON dbo.ADM_RegistrationDemo (GroupCode, BranchCode, SessionId);
END
GO

-- ============================================================
-- 2. Stored Procedure: ADM_UspAddRegistrationDemo
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.ADM_UspAddRegistrationDemo
    @GroupCode   VARCHAR(20),
    @BranchCode  VARCHAR(20),
    @SessionId   BIGINT,
    @StudentName VARCHAR(150),
    @CreatedBy   VARCHAR(50),
    @RegiNo      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ADM_RegistrationDemo (GroupCode, BranchCode, SessionId, StudentName, CreatedBy)
    VALUES (@GroupCode, @BranchCode, @SessionId, @StudentName, @CreatedBy);

    SET @RegiNo = SCOPE_IDENTITY();
END
GO

-- ============================================================
-- 3. Stored Procedure: ADM_UspGetRegistrationDemo (single record)
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.ADM_UspGetRegistrationDemo
    @GroupCode  VARCHAR(20),
    @BranchCode VARCHAR(20),
    @SessionId  BIGINT,
    @RegiNo     INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RegiNo, GroupCode, BranchCode, SessionId, StudentName, CreatedBy, CreatedDate, IsValid
    FROM dbo.ADM_RegistrationDemo
    WHERE GroupCode = @GroupCode
      AND BranchCode = @BranchCode
      AND RegiNo = @RegiNo
      AND IsValid = 1;
END
GO

-- ============================================================
-- 4. Stored Procedure: ADM_UspGetRegistrationDemoList
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.ADM_UspGetRegistrationDemoList
    @GroupCode  VARCHAR(20),
    @BranchCode VARCHAR(20),
    @SessionId  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RegiNo, GroupCode, BranchCode, SessionId, StudentName, CreatedBy, CreatedDate, IsValid
    FROM dbo.ADM_RegistrationDemo
    WHERE GroupCode = @GroupCode
      AND BranchCode = @BranchCode
      AND IsValid = 1
    ORDER BY RegiNo DESC;
END
GO

/*
-- ============================================================
-- ROLLBACK (run manually if this feature needs to be removed)
-- ============================================================
DROP PROCEDURE IF EXISTS dbo.ADM_UspGetRegistrationDemoList;
DROP PROCEDURE IF EXISTS dbo.ADM_UspGetRegistrationDemo;
DROP PROCEDURE IF EXISTS dbo.ADM_UspAddRegistrationDemo;
DROP TABLE IF EXISTS dbo.ADM_RegistrationDemo;
*/
