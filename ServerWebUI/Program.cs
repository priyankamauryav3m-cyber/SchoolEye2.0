
using ApexCharts;
using DomainModel.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using ServerWebUI.Components;
using ServerWebUI.Components.Common;
using ServerWebUI.Components.CommonClass;
using ServerWebUI.Shared;
using System.Security.Claims;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddLocalization();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddScoped<ProtectedSessionStorage>();

    builder.Services.Configure<ApplicationConfiguration>(
        builder.Configuration.GetSection("AppConfigurationSettings"));

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"C:\SharedAuthKeys"))
        .SetApplicationName("LoginV3M");

    builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options =>
        {
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
            options.DetailedErrors = true;
        });

    builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
    builder.Services.AddScoped<SweetAlertService>();
    builder.Services.AddScoped<ICommonMethod, CommonMethod>();
    builder.Services.AddApexCharts();
    builder.Services.AddScoped<PermissionState>();
    builder.Services.AddScoped<BookmarkState>();
    builder.Services.AddScoped<CommonMethod>();
    builder.Services.AddScoped<MenuState>();
    builder.Services.AddScoped<PageLoadingState>();
    builder.Services.AddScoped<NotificationState>();
    builder.Services.AddSingleton<SecureUrlService>();
    builder.Services.AddSingleton<IUILogService, UILogService>();
    builder.Services.AddScoped(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;

        var client = new HttpClient
        {
            BaseAddress = new Uri(settings.LocalAPIURL),
            Timeout = TimeSpan.FromMinutes(10) 
        };

        return client;
    });
    builder.Services.AddHttpClient<IHttpService, HttpService>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;

        client.BaseAddress = new Uri(settings.LocalAPIURL);
        client.Timeout = TimeSpan.FromMinutes(60);
    });

    // AUTHENTICATION
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Google";
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "LoginV3M.Auth";
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;

        options.Events.OnTicketReceived = async context =>
        {
            try
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;

                var email = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = context.Principal.FindFirst(ClaimTypes.Name)?.Value;

                identity.AddClaim(new Claim("LoginProvider", "Google"));

                await context.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    context.Principal);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await context.HttpContext.RequestServices.GetRequiredService<IUILogService>().LogErrorAsync("Login", "Google login error", ex);
            }
        };
    })
    .AddFacebook("Facebook", options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.CallbackPath = "/signin-facebook";
        options.SaveTokens = true;

        options.Events.OnTicketReceived = async context =>
        {
            try
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;

                var email = context.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = context.Principal.FindFirst(ClaimTypes.Name)?.Value;

                identity.AddClaim(new Claim("LoginProvider", "Facebook"));

                await context.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    context.Principal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await context.HttpContext.RequestServices.GetRequiredService<IUILogService>().LogErrorAsync("Login", "Facebook login error", ex);
            }
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    // GOOGLE LOGIN ENDPOINT
    app.MapGet("/login", async (HttpContext http) =>
    {
        try
        {
            
            var provider = http.Request.Query["provider"].ToString();

            var redirectUri = $"{http.Request.Scheme}://{http.Request.Host}/googel-facebook-login";
            //var redirectUri = $"{http.Request.Scheme}://{http.Request.Host}/";

            await http.ChallengeAsync(provider, new AuthenticationProperties
            {
                RedirectUri = redirectUri
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await http.RequestServices.GetRequiredService<IUILogService>().LogErrorAsync("Login", "Social login challenge error. Provider: " + http.Request.Query["provider"], ex);
        }
    });
    app.MapGet("/logout", async (HttpContext context) =>
    {
        try
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);

            return Results.Redirect("/");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    });

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
