using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public interface ICondotelRepository
    {
        IEnumerable<Condotel> GetCondtels();
        Condotel GetCondotelById(int id);
        void AddCondotel(Condotel condotel);
        void UpdateCondotel(Condotel condotel);
        void DeleteCondotel(int id);
        bool SaveChanges();
        Promotion? GetPromotionById(int promotionId);
    }
}
