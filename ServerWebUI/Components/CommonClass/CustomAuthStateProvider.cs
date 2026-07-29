using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public ClaimsPrincipal _currentUser =
        new ClaimsPrincipal(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var claims = jwt.Claims.ToList();

        // 👇 FIX: remap role claim properly
        var roleClaims = claims
            .Where(c => c.Type == "role")
            .Select(c => new Claim(ClaimTypes.Role, c.Value))
            .ToList();

        claims.RemoveAll(c => c.Type == "role");
        claims.AddRange(roleClaims);

        var identity = new ClaimsIdentity(
            claims,
            "jwt",
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_currentUser)));
    }
}

//public class CustomAuthStateProvider : AuthenticationStateProvider
//{
//    private readonly IJSRuntime _js;


//    public CustomAuthStateProvider(IJSRuntime js)
//    {
//        _js = js;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        try
//        {
//            var token = await _js.InvokeAsync<string>(
//                "localStorage.getItem", "authToken");

//            if (string.IsNullOrWhiteSpace(token))
//            {
//                return new AuthenticationState(
//                    new ClaimsPrincipal(new ClaimsIdentity()));
//            }

//            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
//            var claims = jwt.Claims.ToList();

//            // 🔥 IMPORTANT
//            var identity = new ClaimsIdentity(
//                claims,
//                authenticationType: "jwt",
//                nameType: ClaimTypes.Name,
//                roleType: ClaimTypes.Role
//            );



//            var user = new ClaimsPrincipal(identity);

//            return new AuthenticationState(user);
//        }
//        catch
//        {
//            return new AuthenticationState(
//                new ClaimsPrincipal(new ClaimsIdentity()));
//        }
//    }

//    // 🔥 Call this after login
//    public void NotifyUserAuthentication(string token)
//    {
//        try
//        {
//            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

//            // Convert short "role" claim to long schema
//            var claims = new List<Claim>();

//            foreach (var claim in jwt.Claims)
//            {
//                if (claim.Type == "role")
//                {
//                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
//                }
//                else
//                {
//                    claims.Add(claim);
//                }
//            }

//            var identity = new ClaimsIdentity(
//                claims,
//                "jwt",
//                ClaimTypes.Name,
//                ClaimTypes.Role
//            );

//            var user = new ClaimsPrincipal(identity);

//            NotifyAuthenticationStateChanged(
//                Task.FromResult(new AuthenticationState(user)));

//        }
//        catch { }
//    }

//    // 🔥 Call this on logout
//    public void NotifyUserLogout()
//    {
//        NotifyAuthenticationStateChanged(
//            Task.FromResult(new AuthenticationState(
//                new ClaimsPrincipal(new ClaimsIdentity()))));
//    }
//}
