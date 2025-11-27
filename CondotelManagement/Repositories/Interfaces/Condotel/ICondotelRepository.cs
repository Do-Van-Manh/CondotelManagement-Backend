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

        IEnumerable<Condotel> GetCondtelsByHost(int hostId);
		IEnumerable<Condotel> GetCondotelsByFilters(
			string? name,
			string? location,
			DateOnly? fromDate,
			DateOnly? toDate,
			decimal? minPrice,
			decimal? maxPrice,
			int? beds,
			int? bathrooms);

		// Validation methods
		bool ResortExists(int? resortId);
		bool AmenitiesExist(List<int>? amenityIds);
		bool UtilitiesExist(List<int>? utilityIds);
		bool UtilitiesBelongToHost(List<int>? utilityIds, int hostId);
		bool HostExists(int hostId);
		
		// Methods for adding child entities
		void AddCondotelImages(IEnumerable<CondotelImage> images);
		void AddCondotelPrices(IEnumerable<CondotelPrice> prices);
		void AddCondotelDetails(IEnumerable<CondotelDetail> details);
		void AddCondotelAmenities(IEnumerable<CondotelAmenity> amenities);
		void AddCondotelUtilities(IEnumerable<CondotelUtility> utilities);
	}
}
