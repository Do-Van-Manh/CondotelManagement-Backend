using CondotelManagement.DTOs;
using CondotelManagement.Models;

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
        IEnumerable<CondotelDTO> GetCondotelsByNameLocationAndDate( string? name, string? location, DateOnly? fromDate, DateOnly? toDate);
	}
}
