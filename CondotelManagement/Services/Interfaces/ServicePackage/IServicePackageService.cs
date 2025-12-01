using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
    public interface IServicePackageService
    {
        Task<IEnumerable<ServicePackageDTO>> GetAllByHostAsync(int hostId);
        Task<ServicePackageDTO?> GetByIdAsync(int id);
        Task<ServicePackageDTO> CreateAsync(int hostId, CreateServicePackageDTO dto);
        Task<ServicePackageDTO?> UpdateAsync(int id, UpdateServicePackageDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
