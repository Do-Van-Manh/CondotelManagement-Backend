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
using CondotelManagement.Services.Implementations.Blog;
using CondotelManagement.Services.Implementations.Shared;
using CondotelManagement.Services.Implementations.Tenant;
using CondotelManagement.Services.Interfaces;
using CondotelManagement.Services.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Blog;
using CondotelManagement.Services.Interfaces.BookingService;
using CondotelManagement.Services.Interfaces.Shared;
using CondotelManagement.Services.Interfaces.Tenant;
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

            // --- Đăng ký Dependency Injection (DI) ---
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // --- Admin ---
            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            // --- Admin User Management ---
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserService, UserService>();

            // --- Auth ---
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAuthService, AuthService>();

            // --- Shared ---
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IProfileService, ProfileService>();

            // --- Condotel ---
            services.AddScoped<ICondotelRepository, CondotelRepository>();
            services.AddScoped<ICondotelService, CondotelService>();

            // --- Location ---
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ILocationService, LocationService>();

            // --- Promotion ---
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IPromotionService, PromotionService>();

            // --- Host ---
            services.AddScoped<IHostRepository, HostRepository>();
            services.AddScoped<IHostService, HostService>();

            // --- Booking ---
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

            // --- Tenant Review & Reward ---
            services.AddScoped<ITenantReviewService, TenantReviewService>();
            services.AddScoped<ITenantRewardService, TenantRewardService>();

            // --- Customer ---
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerService, CustomerService>();

            // --- Service Package ---
            services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
            services.AddScoped<IServicePackageService, ServicePackageService>();

            // --- Host Report ---
            services.AddScoped<IHostReportRepository, HostReportRepository>();
            services.AddScoped<IHostReportService, HostReportService>();

			// --- Voucher ---
			services.AddScoped<IVoucherRepository, VoucherRepository>();
			services.AddScoped<IVoucherService, VoucherService>();
            // --- Blog (THÊM MỚI) ---
            services.AddScoped<IBlogService, BlogService>();

			// --- Review ---
			services.AddScoped<IReviewRepository, ReviewRepository>();
			services.AddScoped<IReviewService, ReviewService>();

            // --- 2. THEM CAC DONG MOI O DAY ---
            // Dang ky Service cho Package
            services.AddScoped<IPackageService, PackageService>();
            // Dang ky Service cho Quyen loi (Singleton vi no la hard-code, khong doi)
            services.AddSingleton<IPackageFeatureService, PackageFeatureService>();

            // --- Cấu hình JWT Authentication ---
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
