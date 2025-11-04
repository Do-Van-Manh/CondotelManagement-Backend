using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<Promotion>> GetAllAsync();
        Task<Promotion?> GetByIdAsync(int id);
        Task<IEnumerable<Promotion>> GetByCondotelIdAsync(int condotelId);
        Task<Promotion> AddAsync(Promotion promotion);
        Task UpdateAsync(Promotion promotion);
        Task DeleteAsync(Promotion promotion);
    }
}



