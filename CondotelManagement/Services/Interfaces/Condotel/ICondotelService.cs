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
        IEnumerable<CondotelDTO> GetCondotelsByFilters(
            string? name, 
            string? location, 
            DateOnly? fromDate, 
            DateOnly? toDate, 
            decimal? minPrice,
	        decimal? maxPrice,
	        int? beds,
	        int? bathrooms);
	}
}
