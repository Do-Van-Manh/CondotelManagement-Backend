using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public interface IServicePackageRepository
    {
        Task<IEnumerable<ServicePackage>> GetAllAsync();
        Task<ServicePackage?> GetByIdAsync(int id);
        Task AddAsync(ServicePackage entity);
        Task UpdateAsync(ServicePackage entity);
        Task SaveAsync();
    }
}
