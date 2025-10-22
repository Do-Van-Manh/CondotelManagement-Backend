using CondotelManagement.Repositories.Implementations.Admin;
using CondotelManagement.Repositories.Implementations.Auth;
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services.Implementations.Admin;
using CondotelManagement.Services.Implementations.Auth;
using CondotelManagement.Services.Implementations.Shared;
using CondotelManagement.Services.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Cho JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;             // Cho TokenValidationParameters, SymmetricSecurityKey
using System.Text;                                // Cho Encoding
namespace CondotelManagement.Configurations

{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            //DI 
            // ae dki service cac thu trong day
            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            // Auth
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAuthService, AuthService>();
            // Cấu hình JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],       // Cần thêm "Jwt:Issuer" trong appsettings.json
                    ValidAudience = configuration["Jwt:Audience"],   // Cần thêm "Jwt:Audience" trong appsettings.json
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])) // Key của bạn
                };
            });

            // Thêm Email Service
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
