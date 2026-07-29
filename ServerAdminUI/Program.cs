
using AuthUI.Components;
using Microsoft.AspNetCore.Authentication.Cookies;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();
//builder.Services.AddHttpClient("Gateway", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7289/");
//});
//var app = builder.Build();


//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    app.UseHsts();
//}
//app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();

//app.UseAntiforgery();

//app.MapStaticAssets();
//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

//app.Run();

var builder = WebApplication.CreateBuilder(args);

// ---------- Blazor Server (interactive server-side rendering) ----------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---------- Cookie authentication for the whole app; login/2FA is handled by ----------
// ---------- the minimal API endpoints in AccountEndpoints.cs, not by Blazor forms ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SchoolEye.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// A short-lived data-protection-backed cookie carries the userId between the password step
// and the OTP step, without fully signing the user in until 2FA succeeds.
builder.Services.AddDataProtection();

// Base address of the AAFT.LMS.Api project - update in appsettings.json per environment.
// This is a server-to-server call (Blazor Server host -> Web API), so no CORS is required
// on this leg; the API's CORS policy only matters if it is also called from a browser client.

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5205/";
builder.Services.AddHttpClient("ServerWebAPI", c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerWebAPI"));

// Global loading-overlay service used by every page ("waiting image" requirement).
//builder.Services.AddScoped<LoadingService>();
//builder.Services.AddScoped<ToastService>();
//builder.Services.AddScoped<ApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

AuthUI.AccountEndpoints.Map(app);


app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

