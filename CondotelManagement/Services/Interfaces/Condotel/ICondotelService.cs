using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
    public interface ICondotelService
    {
        IEnumerable<CondotelDTO> GetCondotels();
        CondotelDetailDTO GetCondotelById(int id);
        CondotelDetailDTO CreateCondotel(CondotelDetailDTO condotel);
        CondotelDetailDTO UpdateCondotel(CondotelDetailDTO condotel);
        bool DeleteCondotel(int id);
    }
}
