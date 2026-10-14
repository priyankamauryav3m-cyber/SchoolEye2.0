/*
    Forgot password — one input "Username or email" (run after PasswordReset_Migration.sql)
    ----------------------------------------------------------------
    usp_Auth_GetPasswordResetUsers takes @Login (username OR email) and returns ONE valid user with an email:
      1. a user whose UserName = @Login, else
      2. the user whose EmailId = @Login — only when exactly one valid user has that email
         (a shared email can not tell whose password to reset, so no link is sent)
    Nothing returned -> no link is sent (the API reply stays the same either way).

    Caller  : Infrastructure/User/UserService.cs SendPasswordResetLinkAsync (only caller)
    Safety  : ALTER of a read-only procedure created by PasswordReset_Migration.sql. No data is changed.
              Safe to run again (CREATE OR ALTER).
    Rollback: run section 2 of PasswordReset_Migration.sql again (the @EmailId only version).
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Auth_GetPasswordResetUsers
(
    @Login VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Login = LTRIM(RTRIM(@Login));

    -- same validity rules as login (IsValid + ValidFrom / ValidTo); no email = no link
    DECLARE @Valid TABLE (UserSid INT, UserName VARCHAR(100), EmailId VARCHAR(50));
    INSERT INTO @Valid (UserSid, UserName, EmailId)
    SELECT UserSid, UserName, EmailId
    FROM dbo.MstUsers WITH (NOLOCK)
    WHERE IsValid = 1
      AND GETDATE() >= ValidFrom
      AND (ValidTo IS NULL OR GETDATE() <= ValidTo)
      AND ISNULL(EmailId, '') <> ''
      AND (UserName COLLATE DATABASE_DEFAULT = @Login COLLATE DATABASE_DEFAULT
           OR LOWER(LTRIM(RTRIM(EmailId))) = LOWER(@Login));

    -- 1. username
    IF EXISTS (SELECT 1 FROM @Valid WHERE UserName COLLATE DATABASE_DEFAULT = @Login COLLATE DATABASE_DEFAULT)
    BEGIN
        SELECT TOP 1 UserSid, UserName, EmailId
        FROM @Valid
        WHERE UserName COLLATE DATABASE_DEFAULT = @Login COLLATE DATABASE_DEFAULT;
        RETURN;
    END

    -- 2. email of exactly one user
    IF (SELECT COUNT(*) FROM @Valid) = 1
        SELECT UserSid, UserName, EmailId FROM @Valid;
END
GO
