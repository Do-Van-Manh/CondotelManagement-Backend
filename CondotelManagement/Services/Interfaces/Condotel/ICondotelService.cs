using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
    public interface ICondotelService
    {
        IEnumerable<CondotelDTO> GetCondotels();
		CondotelDetailDTO GetCondotelById(int id);
        CondotelCreateUpdateDTO CreateCondotel(CondotelCreateUpdateDTO condotel);
        CondotelCreateUpdateDTO UpdateCondotel(CondotelCreateUpdateDTO condotel);
        bool DeleteCondotel(int id);
    }
}
