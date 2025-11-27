using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
    public interface IResortService
    {
        Task<IEnumerable<ResortDTO>> GetAllAsync();
        Task<ResortDTO?> GetByIdAsync(int id);
        Task<IEnumerable<ResortDTO>> GetByLocationIdAsync(int locationId);
    }
}









