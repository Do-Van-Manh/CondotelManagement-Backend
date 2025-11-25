using CondotelManagement.Data;
using CondotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly CondotelDbVer1Context _context;

        public PromotionRepository(CondotelDbVer1Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            return await _context.Promotions
                .Include(p => p.Condotel)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions
                .Include(p => p.Condotel)
                .FirstOrDefaultAsync(p => p.PromotionId == id);
        }

        public async Task<IEnumerable<Promotion>> GetByCondotelIdAsync(int condotelId)
        {
            return await _context.Promotions
                .Include(p => p.Condotel)
                .Where(p => p.CondotelId == condotelId)
                .ToListAsync();
        }

        public async Task<Promotion> AddAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task UpdateAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Promotion promotion)
        {
            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }

		public async Task<bool> CheckOverlapAsync(int? condotelId, DateOnly startDate, DateOnly endDate)
		{
			return await _context.Promotions.AnyAsync(p =>
				p.CondotelId == condotelId &&
				p.StartDate <= endDate && p.EndDate >= startDate
			);
		}

		public async Task<IEnumerable<Promotion>> GetAllByHostAsync(int hostId)
		{
			return await _context.Promotions
				.Include(p => p.Condotel)
				.Where(v => v.Condotel != null && v.Condotel.HostId == hostId)
				.ToListAsync();
		}
	}
}








