using ApplicationInterface.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Infrastructure.Configuration;
using Infrastructure.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.User
{
    public static class DependencyInjection
    {
        private const string DATABASE_SETTINGS_KEY = "DatabaseSettings";

        // public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        public static IServiceCollection AddInfrastructure(this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDatabase(configuration)
             .AddAuthenticationService(configuration)
             .AddServices();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUser, UserService>();
            ;
            services.AddScoped<IOtpSenderService, OtpSender>();
            return services;
            //    .AddScoped<IValidationService, ValidationService>()
            //    .AddScoped<IDateTime, DateTimeService>()
            //    .AddScoped<IExcelService, ExcelService>()
            //    .AddScoped<IUploadService, UploadService>()
            //    .AddScoped<IPDFService, PDFService>()
            //    .AddTransient<IDocumentOcrJob, DocumentOcrJob>();
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services,
       IConfiguration configuration)
        {

            Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions.Configure<DatabaseSettings>(services, configuration.GetSection("DatabaseSettings"));

            services.Configure<DatabaseSettings>(configuration.GetSection(DATABASE_SETTINGS_KEY))
            .AddSingleton(s => s.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            return services;

        }

        private static IServiceCollection AddAuthenticationService(this IServiceCollection services,
       IConfiguration configuration)
        {
            Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions.Configure<JsonWebToken>(services, configuration.GetSection("Jwt"));
            services.Configure<JsonWebToken>(configuration.GetSection("Jwt"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                //.AddJwtBearer(options =>
                //{
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        ValidateIssuer = true,
                //        ValidateAudience = true,
                //        ValidateLifetime = true,
                //        ValidateIssuerSigningKey = true,
                //        ValidIssuer = configuration["Jwt:Issuer"],
                //        ValidAudience = configuration["Jwt:Issuer"],
                //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                //    };
                //});

                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),

                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            return services;
        }
    }
}
