using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
    public interface ICondotelService
    {
        IEnumerable<CondotelDTO> GetCondotels();
		CondotelDetailDTO GetCondotelById(int id);
        CondotelUpdateDTO CreateCondotel(CondotelCreateDTO condotel);
        CondotelUpdateDTO UpdateCondotel(CondotelUpdateDTO condotel);
        bool DeleteCondotel(int id);
        IEnumerable<CondotelDTO> GetCondtelsByHost(int hostId);
    }
}
