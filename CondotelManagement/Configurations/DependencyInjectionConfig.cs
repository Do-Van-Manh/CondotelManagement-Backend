using CondotelManagement.Repositories.Implementations;
using CondotelManagement.Repositories.Implementations.Admin;
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Services.Implementations.Admin;
using CondotelManagement.Services.Interfaces.Admin;
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
        }
    }
}
