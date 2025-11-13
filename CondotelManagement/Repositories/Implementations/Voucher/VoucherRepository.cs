using CondotelManagement.Data;
using CondotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CondotelManagement.Repositories
{
	public class VoucherRepository : IVoucherRepository
	{
		private readonly CondotelDbVer1Context _context;

		public VoucherRepository(CondotelDbVer1Context context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Voucher>> GetByHostAsync(int hostId)
		{
			return await _context.Vouchers
				.Include(v => v.Condotel)
				.Include(v => v.User)
				.Where(v => v.Condotel != null && v.Condotel.HostId == hostId)
				.ToListAsync();
		}

		public async Task<IEnumerable<Voucher>> GetByCondotelAsync(int condotelId)
		{
			return await _context.Vouchers
				.Include(v => v.Condotel)
				.Include(v => v.User)
				.Where(v => v.CondotelId == condotelId && v.Status == "Active")
				.ToListAsync();
		}

		public async Task<Voucher?> GetByIdAsync(int id)
		{
			return await _context.Vouchers.FindAsync(id);
		}

		public async Task<Voucher> AddAsync(Voucher voucher)
		{
			_context.Vouchers.Add(voucher);
			await _context.SaveChangesAsync();
			// Load lại để có Condotel & User
			var saved = await _context.Vouchers
				.Include(v => v.Condotel)
				.Include(v => v.User)
				.FirstOrDefaultAsync(v => v.VoucherId == voucher.VoucherId);
			return saved;
		}

		public async Task<Voucher?> UpdateAsync(Voucher voucher)
		{
			var existing = await _context.Vouchers.FindAsync(voucher.VoucherId);
			if (existing == null) return null;

			_context.Entry(existing).CurrentValues.SetValues(voucher);
			await _context.SaveChangesAsync();
			// Load lại để có Condotel & User
			var saved = await _context.Vouchers
				.Include(v => v.Condotel)
				.Include(v => v.User)
				.FirstOrDefaultAsync(v => v.VoucherId == existing.VoucherId);
			return saved;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var existing = await _context.Vouchers.FindAsync(id);
			if (existing == null) return false;

			_context.Vouchers.Remove(existing);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<string> GenerateUniqueVoucherCodeAsync(int userId, int maxRetries = 5)
		{
			for (int i = 0; i < maxRetries; i++)
			{
				// Tạo code dạng: BOOK + UserID + 6 ký tự random (chữ + số)
				var randomPart = Path.GetRandomFileName().Replace(".", "").Substring(0, 6).ToUpper();
				string code = $"BOOK{userId}{randomPart}";

				// Kiểm tra đã tồn tại trong DB chưa
				bool exists = await _context.Vouchers.AnyAsync(v => v.Code == code);
				if (!exists)
					return code; // unique -> trả về

				// nếu trùng -> retry
			}

			throw new Exception("Cannot generate unique voucher code after multiple attempts");
		}
	}
}
