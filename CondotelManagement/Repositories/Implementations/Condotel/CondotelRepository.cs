using CondotelManagement.Data;
using CondotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories
{
    public class CondotelRepository : ICondotelRepository
    {
        private readonly CondotelDbVer1Context _context;

        public CondotelRepository(CondotelDbVer1Context context)
        {
            _context = context;
        }
        public void AddCondotel(Condotel condotel)
        {
            _context.Condotels.Add(condotel);
        }

    public void DeleteCondotel(int id)
    {
        var condotel = _context.Condotels
            .Include(c => c.CondotelImages)
            .Include(c => c.CondotelAmenities)
            .Include(c => c.CondotelPrices)
            .Include(c => c.CondotelDetails)
            .Include(c => c.CondotelUtilities)
            .FirstOrDefault(c => c.CondotelId == id);

        if (condotel != null)
        {
            _context.CondotelImages.RemoveRange(condotel.CondotelImages);
            _context.CondotelAmenities.RemoveRange(condotel.CondotelAmenities);
            _context.CondotelPrices.RemoveRange(condotel.CondotelPrices);
            _context.CondotelDetails.RemoveRange(condotel.CondotelDetails);
            _context.CondotelUtilities.RemoveRange(condotel.CondotelUtilities);
            _context.Condotels.Remove(condotel);
        }
    }

    public Condotel GetCondotelById(int id)
    {
        return _context.Condotels
            .Include(r => r.Resort)
            .Include(c => c.CondotelImages)
            .Include(c => c.CondotelAmenities)
			.ThenInclude(ca => ca.Amenity)
			.Include(c => c.CondotelPrices)
            .Include(c => c.CondotelDetails)
            .Include(c => c.CondotelUtilities)
			.ThenInclude(cu => cu.Utility)
			.Include(c => c.Promotions)
			.FirstOrDefault(c => c.CondotelId == id);
    }

        public IEnumerable<Condotel> GetCondtels()
        {
            return _context.Condotels
                .Include(c => c.Resort)
                .Include(c => c.Host)
                .Include(c => c.CondotelImages)
                .ToList();
        }

    public void UpdateCondotel(Condotel condotel)
    {
        // Xóa và thêm lại các bảng con nếu có
        var existing = _context.Condotels
            .Include(c => c.CondotelImages)
            .Include(c => c.CondotelAmenities)
            .Include(c => c.CondotelPrices)
            .Include(c => c.CondotelDetails)
            .Include(c => c.CondotelUtilities)
            .FirstOrDefault(c => c.CondotelId == condotel.CondotelId);

        if (existing != null)
        {
            // Xóa dữ liệu cũ
            _context.CondotelImages.RemoveRange(existing.CondotelImages);
            _context.CondotelAmenities.RemoveRange(existing.CondotelAmenities);
            _context.CondotelPrices.RemoveRange(existing.CondotelPrices);
            _context.CondotelDetails.RemoveRange(existing.CondotelDetails);
            _context.CondotelUtilities.RemoveRange(existing.CondotelUtilities);

            // Thêm dữ liệu mới
            if (condotel.CondotelImages != null)
                _context.CondotelImages.AddRange(condotel.CondotelImages);

            if (condotel.CondotelAmenities != null)
                _context.CondotelAmenities.AddRange(condotel.CondotelAmenities);

            if (condotel.CondotelPrices != null)
                _context.CondotelPrices.AddRange(condotel.CondotelPrices);

            if (condotel.CondotelDetails != null)
                _context.CondotelDetails.AddRange(condotel.CondotelDetails);

            if (condotel.CondotelUtilities != null)
                _context.CondotelUtilities.AddRange(condotel.CondotelUtilities);
        }
    }
        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
