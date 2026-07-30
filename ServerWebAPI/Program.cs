using Infrastructure.User;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using ServerWebAPI.Authorization;
using System.Globalization;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);

#region Services
//builder.Services.AddApplication();
builder.Services.AddControllers();
Infrastructure.User.DependencyInjection.AddInfrastructure(builder.Services,
    builder.Configuration);
#endregion



#region Authentication (Google + Facebook + Cookie)

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

//})
//.AddCookie(options =>
//{
//    options.Cookie.Name = "LoginV3M";
//    options.Cookie.HttpOnly = true;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    options.Cookie.SameSite = SameSiteMode.None;
//    options.SlidingExpiration = true;
//    options.LoginPath = "/login";
//    options.LogoutPath = "/logout";
//})
//.AddGoogle(options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//    options.CallbackPath = "/signin-google";
//})
//.AddFacebook(options =>
//{
//    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
//    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
//    options.CallbackPath = "/signin-facebook";
//});

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("Admission", new OpenApiInfo
    {
        Title = "Admission API",
        Version = "v1"
    });

    c.SwaggerDoc("FinanceManagement", new OpenApiInfo
    {
        Title = "Fee Management API",
        Version = "v1"
    });

    c.SwaggerDoc("Login", new OpenApiInfo
    {
        Title = "Login API",
        Version = "v1"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.GroupName == null)
            return false;

        return apiDesc.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region Localization

builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[]
    {
        new CultureInfo("en-IN"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("en-IN");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

#endregion

#region Rate Limiting

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("V3MLoginAPI_Call_Limit", opt =>
    {
        opt.PermitLimit = Convert.ToInt32(builder.Configuration["V3MLoginAPI_Call_Attempt"]);
        opt.Window = TimeSpan.FromSeconds(
            Convert.ToInt32(builder.Configuration["V3MLoginAPI_Call_InDuration"]));
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("V3MAPI_Call_Limit", opt =>
    {
        opt.PermitLimit = Convert.ToInt32(builder.Configuration["V3MAPI_Call_Limit_Attempt"]);
        opt.Window = TimeSpan.FromSeconds(
            Convert.ToInt32(builder.Configuration["V3MAPI_Call_Limit_InDuration"]));
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken);
    };
});

#endregion

#region Custom Services

builder.Services.RegisterServices();

builder.Services.AddSingleton<SessionManager>();

//builder.Services.AddScoped<Infrastructure.User.IJwtUtils >();
builder.Services.AddScoped<ServerWebAPI.Authorization.IJwtUtils,JwtUtils >();

builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.Configure<Infrastructure.User.SmtpSettings>(
       builder.Configuration.GetSection("Smtp"));


#endregion

#region Data Protection

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\SharedAuthKeys"))
    .SetApplicationName("LoginV3M");

#endregion
#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion
var app = builder.Build();

#region Middleware

app.UseSwagger();

app.UseCors("AllowAll");


app.UseRateLimiter();

app.UseRequestLocalization();


app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("../swagger/Admission/swagger.json", "Admission API");
    c.SwaggerEndpoint("../swagger/FinanceManagement/swagger.json", "Finance API");
    c.SwaggerEndpoint("../swagger/Login/swagger.json", "Login API");
});
#endregion

app.MapControllers();

app.Run();