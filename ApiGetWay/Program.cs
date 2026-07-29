using ApiGetWay.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// =====================
// CORS
// =====================


// =====================
// DB
// =====================
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// IDENTITY CORE
// =====================
builder.Services.AddIdentityCore<AuthenticateUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddSignInManager()
    .AddUserStore<AuthUserStore>()
    .AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("https://localhost:7272")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddAuthentication()
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "Auth.Cookie";
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
//builder.Services
//    .AddAuthentication("Cookies")
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "Auth.Cookie";
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SameSite = SameSiteMode.None;       // 🔥 MUST for cross-port
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 🔥 HTTPS required
//        options.SlidingExpiration = true;
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//        options.LoginPath = "/Account/login";   // IMPORTANT
//    });
builder.Services.AddAuthorization();
// =====================
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<JwtTokenIssuer>();

//builder.Services
//    .AddReverseProxy()
//    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();


app.MapGet("/", () => "Hello World!");

app.Run();
