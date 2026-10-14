/*
    Forgot password — reset link by email (SMTP)
    ----------------------------------------------------------------
    1. Table  V3M_Auth_PasswordResetToken      one-time reset links (only the SHA-256 hash of the token is stored)
    2. SP     usp_Auth_GetPasswordResetUsers    valid users of an email id
    3. SP     usp_Auth_CreatePasswordResetToken new link for a user (old unused links of the user are closed)
    4. SP     usp_Auth_ValidatePasswordResetToken  is the link usable (opened from the email)
    5. SP     usp_Auth_ResetPasswordWithToken   set the new password and close the link, in one transaction

    Callers : Infrastructure/User/UserService.cs (ForgotPassword / ValidateResetToken / ResetPassword)
    Safety  : Additive only. No existing table / procedure is changed.
              MstUsers.UserPassword is updated only by usp_Auth_ResetPasswordWithToken, for the token's own user.
    Rollback: see bottom (commented out).

    Return codes (validate / reset):
      1 = ok, 2 = link not found, 3 = link expired, 4 = link already used / closed, 5 = user not valid
*/

SET NOCOUNT ON;
GO

---------------------------------------------------------------- 1. table
IF OBJECT_ID('dbo.V3M_Auth_PasswordResetToken', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.V3M_Auth_PasswordResetToken
    (
        ResetTokenId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_V3M_Auth_PasswordResetToken PRIMARY KEY,
        UserSid       INT          NOT NULL,
        TokenHash     CHAR(64)     NOT NULL,   -- hex SHA-256 of the token in the link
        ExpiryUtc     DATETIME2    NOT NULL,
        UsedUtc       DATETIME2    NULL,       -- set when the password is changed or the link is closed
        CreatedUtc    DATETIME2    NOT NULL CONSTRAINT DF_V3M_Auth_PasswordResetToken_CreatedUtc DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_V3M_Auth_PasswordResetToken_TokenHash ON dbo.V3M_Auth_PasswordResetToken (TokenHash);
    CREATE INDEX IX_V3M_Auth_PasswordResetToken_UserSid ON dbo.V3M_Auth_PasswordResetToken (UserSid, UsedUtc);
END
GO

---------------------------------------------------------------- 2. users of an email
CREATE OR ALTER PROCEDURE dbo.usp_Auth_GetPasswordResetUsers
(
    @EmailId VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- same validity rules as login (IsValid + ValidFrom / ValidTo)
    SELECT UserSid, UserName, EmailId
    FROM dbo.MstUsers WITH (NOLOCK)
    WHERE EmailId = @EmailId
      AND IsValid = 1
      AND GETDATE() >= ValidFrom
      AND (ValidTo IS NULL OR GETDATE() <= ValidTo)
    ORDER BY UserName;
END
GO

---------------------------------------------------------------- 3. new link
CREATE OR ALTER PROCEDURE dbo.usp_Auth_CreatePasswordResetToken
(
    @UserSid   INT,
    @TokenHash CHAR(64),
    @ExpiryUtc DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;

    -- one link per user per minute (stops mail flooding)
    IF EXISTS (SELECT 1 FROM dbo.V3M_Auth_PasswordResetToken
               WHERE UserSid = @UserSid AND CreatedUtc > DATEADD(MINUTE, -1, SYSUTCDATETIME()))
        RETURN 2;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- a new link closes every older unused link of the user
        UPDATE dbo.V3M_Auth_PasswordResetToken
        SET UsedUtc = SYSUTCDATETIME()
        WHERE UserSid = @UserSid AND UsedUtc IS NULL;

        INSERT INTO dbo.V3M_Auth_PasswordResetToken (UserSid, TokenHash, ExpiryUtc)
        VALUES (@UserSid, @TokenHash, @ExpiryUtc);

        COMMIT TRANSACTION;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        RETURN 0;
    END CATCH
END
GO

---------------------------------------------------------------- 4. validate link
CREATE OR ALTER PROCEDURE dbo.usp_Auth_ValidatePasswordResetToken
(
    @TokenHash CHAR(64)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserSid INT, @ExpiryUtc DATETIME2, @UsedUtc DATETIME2;

    SELECT @UserSid = UserSid, @ExpiryUtc = ExpiryUtc, @UsedUtc = UsedUtc
    FROM dbo.V3M_Auth_PasswordResetToken
    WHERE TokenHash = @TokenHash;

    IF @UserSid IS NULL RETURN 2;
    IF @UsedUtc IS NOT NULL RETURN 4;
    IF @ExpiryUtc < SYSUTCDATETIME() RETURN 3;
    IF NOT EXISTS (SELECT 1 FROM dbo.MstUsers WHERE UserSid = @UserSid AND IsValid = 1) RETURN 5;

    SELECT UserName FROM dbo.MstUsers WHERE UserSid = @UserSid;
    RETURN 1;
END
GO

---------------------------------------------------------------- 5. reset password
CREATE OR ALTER PROCEDURE dbo.usp_Auth_ResetPasswordWithToken
(
    @TokenHash    CHAR(64),
    @UserPassword VARCHAR(50)   -- already encrypted by the API, same format as login
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ResetTokenId BIGINT, @UserSid INT, @ExpiryUtc DATETIME2, @UsedUtc DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- lock the link row so the same link can not be used twice at the same time
        SELECT @ResetTokenId = ResetTokenId, @UserSid = UserSid, @ExpiryUtc = ExpiryUtc, @UsedUtc = UsedUtc
        FROM dbo.V3M_Auth_PasswordResetToken WITH (UPDLOCK, ROWLOCK)
        WHERE TokenHash = @TokenHash;

        IF @ResetTokenId IS NULL BEGIN ROLLBACK TRANSACTION; RETURN 2; END
        IF @UsedUtc IS NOT NULL BEGIN ROLLBACK TRANSACTION; RETURN 4; END
        IF @ExpiryUtc < SYSUTCDATETIME() BEGIN ROLLBACK TRANSACTION; RETURN 3; END

        UPDATE dbo.MstUsers
        SET UserPassword = @UserPassword,
            AccessFailedCount = 0
        WHERE UserSid = @UserSid AND IsValid = 1;

        IF @@ROWCOUNT = 0 BEGIN ROLLBACK TRANSACTION; RETURN 5; END

        -- this link and any other open link of the user are closed
        UPDATE dbo.V3M_Auth_PasswordResetToken
        SET UsedUtc = SYSUTCDATETIME()
        WHERE UserSid = @UserSid AND UsedUtc IS NULL;

        COMMIT TRANSACTION;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        RETURN 0;
    END CATCH
END
GO

/* ---------------- Rollback ----------------
DROP PROCEDURE IF EXISTS dbo.usp_Auth_ResetPasswordWithToken;
DROP PROCEDURE IF EXISTS dbo.usp_Auth_ValidatePasswordResetToken;
DROP PROCEDURE IF EXISTS dbo.usp_Auth_CreatePasswordResetToken;
DROP PROCEDURE IF EXISTS dbo.usp_Auth_GetPasswordResetUsers;
DROP TABLE IF EXISTS dbo.V3M_Auth_PasswordResetToken;
-- note: passwords already changed through a link stay changed
*/
