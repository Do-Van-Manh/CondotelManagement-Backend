// --- CÁC USING CŨ CỦA BẠN ---
using CondotelManagement.Repositories;
using CondotelManagement.Repositories.Implementations; 
using CondotelManagement.Repositories.Implementations.Admin;
using CondotelManagement.Repositories.Implementations.Auth;
using CondotelManagement.Repositories.Interfaces;      
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services;
using CondotelManagement.Services.Implementations;
using CondotelManagement.Services.Implementations.Admin;
using CondotelManagement.Services.Implementations.Auth;
using CondotelManagement.Services.Implementations.Shared;
using CondotelManagement.Services.Interfaces;
using CondotelManagement.Services.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.BookingService;
using CondotelManagement.Services.Interfaces.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace CondotelManagement.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            //DI 
            // ae dki service cac thu trong day

            // Đăng ký Repository (Generic)
           
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // --- Admin ---
            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            // --- Admin User Management (MỚI) ---
            
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserService, UserService>();

            // --- Auth ---
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAuthService, AuthService>();

            // --- Shared ---
            services.AddScoped<IEmailService, EmailService>();
            //---Profile---
            services.AddScoped<IProfileService, ProfileService>();

            //---Booking---
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });
        }
    }
}