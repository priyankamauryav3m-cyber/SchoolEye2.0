using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiGetWay.Authorization
{

    public class AuthUserStore: IUserStore<AuthenticateUser>, IUserPasswordStore<AuthenticateUser>, IUserEmailStore<AuthenticateUser>, IUserSecurityStampStore<AuthenticateUser>, IUserRoleStore<AuthenticateUser>, IUserTwoFactorStore<AuthenticateUser>
    {
        private readonly string _connectionString;

        public AuthUserStore(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection Connection() => new SqlConnection(_connectionString);

        // ================= CREATE =================

        public Task<bool> GetTwoFactorEnabledAsync(AuthenticateUser user, CancellationToken cancellationToken)=> Task.FromResult(user.TwoFactorEnabled);

        public Task SetTwoFactorEnabledAsync(AuthenticateUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }
        public async Task<IdentityResult> CreateAsync(AuthenticateUser user, CancellationToken ct)
        {
            user.Id ??= Guid.NewGuid().ToString();
            user.SecurityStamp ??= Guid.NewGuid().ToString();
            user.ConcurrencyStamp ??= Guid.NewGuid().ToString();
            user.EmailConfirmed = user.EmailConfirmed;
            user.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            user.TwoFactorEnabled = user.TwoFactorEnabled;
            user.LockoutEnabled = user.LockoutEnabled;
            user.AccessFailedCount = user.AccessFailedCount;

            var sql = @"INSERT INTO Users
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                     PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, LockoutEnabled, TwoFactorEnabled, AccessFailedCount)
                    VALUES
                    (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                     @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumberConfirmed, @LockoutEnabled, @TwoFactorEnabled, @AccessFailedCount)";

            using var conn = Connection();
            await conn.ExecuteAsync(sql, user, commandTimeout: 30);
            return IdentityResult.Success;
        }

        // ================= FIND =================
        public async Task<AuthenticateUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
        {
            var sql = "select *from Users ut join AspNetUserRoles rm on ut.id=rm.UserId  where NormalizedUserName = @normalizedUserName";
            using var conn = Connection();
            return await conn.QuerySingleOrDefaultAsync<AuthenticateUser>(sql, new { normalizedUserName });
        }

        public async Task<AuthenticateUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        {
            const string sql = @"SELECT CAST(Id AS varchar(36)) AS Id, UserName, NormalizedUserName, Email,NormalizedEmail,PasswordHash
                                FROM Users
                                WHERE NormalizedEmail = @normalizedEmail";

            using var conn = Connection();

            return await conn.QuerySingleOrDefaultAsync<AuthenticateUser>(
                sql,
                new { normalizedEmail }
            );
        }


        public async Task<AuthenticateUser?> FindByIdAsync(string id, CancellationToken ct)
        {
            var sql = "SELECT * FROM Users WHERE Id = @id";
            using var conn = Connection();
            return await conn.QuerySingleOrDefaultAsync<AuthenticateUser>(sql, new { id });
        }

        // ================= PASSWORD =================
        public Task<string?> GetPasswordHashAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

        public Task SetPasswordHashAsync(AuthenticateUser user, string hash, CancellationToken ct)
        {
            user.PasswordHash = hash;
            return Task.CompletedTask;
        }

        // ================= USER =================
        public Task<string?> GetUserIdAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(AuthenticateUser user, string name, CancellationToken ct)
        {
            user.UserName = name;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(AuthenticateUser user, string name, CancellationToken ct)
        {
            user.NormalizedUserName = name;
            return Task.CompletedTask;
        }

        // ================= EMAIL =================
        public Task SetEmailAsync(AuthenticateUser user, string email, CancellationToken ct)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetEmailAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.Email);

        public Task<string?> GetNormalizedEmailAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(AuthenticateUser user, string email, CancellationToken ct)
        {
            user.NormalizedEmail = email;
            return Task.CompletedTask;
        }

        public Task<bool> GetEmailConfirmedAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(AuthenticateUser user, bool confirmed, CancellationToken ct)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task<string?> GetSecurityStampAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(user.SecurityStamp);

        public Task SetSecurityStampAsync(AuthenticateUser user, string stamp, CancellationToken ct)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(AuthenticateUser user, CancellationToken ct)
        {
            var sql = @"UPDATE Users SET
                UserName=@UserName,
                NormalizedUserName=@NormalizedUserName,
                Email=@Email,
                NormalizedEmail=@NormalizedEmail,
                EmailConfirmed=@EmailConfirmed,
                PasswordHash=@PasswordHash,
                SecurityStamp=@SecurityStamp,
                ConcurrencyStamp=@ConcurrencyStamp
                WHERE Id=@Id";

            using var conn = Connection();
            await conn.ExecuteAsync(sql, user);
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(AuthenticateUser user, CancellationToken ct)
            => Task.FromResult(IdentityResult.Success);

        public void Dispose() { }

        // ================= USER ROLES =================
        public async Task AddToRoleAsync(AuthenticateUser user, string roleName, CancellationToken ct)
        {
            var sql = @"INSERT INTO AspNetUserRoles(UserId, RoleId)
                        VALUES(@UserId, (SELECT Id FROM AspNetRoles WHERE Name=@RoleName))";
            using var conn = Connection();
            await conn.ExecuteAsync(sql, new { UserId = user.Id, RoleName = roleName });
        }

        public async Task RemoveFromRoleAsync(AuthenticateUser user, string roleName, CancellationToken ct)
        {
            var sql = @"DELETE FROM AspNetUserRoles
                        WHERE UserId=@UserId AND RoleId=(SELECT Id FROM AspNetRoles WHERE Name=@RoleName)";
            using var conn = Connection();
            await conn.ExecuteAsync(sql, new { UserId = user.Id, RoleName = roleName });
        }

        public async Task<IList<string>> GetRolesAsync(AuthenticateUser user, CancellationToken ct)
        {
            var sql = @"SELECT r.Name FROM AspNetRoles r INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId WHERE ur.UserId=@UserId";
            using var conn = Connection();
            var roles = await conn.QueryAsync<string>(sql, new { UserId = user.Id });
            return roles.ToList();
        }

        public async Task<bool> IsInRoleAsync(AuthenticateUser user, string roleName, CancellationToken ct)
        {
            var sql = @"SELECT COUNT(*) FROM AspNetRoles r
                        INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                        WHERE ur.UserId=@UserId AND r.Name=@RoleName";
            using var conn = Connection();
            var count = await conn.ExecuteScalarAsync<int>(sql, new { UserId = user.Id, RoleName = roleName });
            return count > 0;
        }

        public async Task<IList<AuthenticateUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct)
        {
            var sql = @"SELECT u.* FROM Users u
                        INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                        WHERE r.Name=@RoleName";
            using var conn = Connection();
            var users = await conn.QueryAsync<AuthenticateUser>(sql, new { RoleName = roleName });
            return users.ToList();
        }
    }
}
