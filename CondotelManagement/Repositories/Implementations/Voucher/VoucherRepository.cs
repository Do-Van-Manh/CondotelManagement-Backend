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
				.Where(v => v.Condotel != null && v.Condotel.HostId == hostId)
				.ToListAsync();
		}

		public async Task<IEnumerable<Voucher>> GetByCondotelAsync(int condotelId)
		{
			return await _context.Vouchers
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
			return voucher;
		}

		public async Task<Voucher?> UpdateAsync(Voucher voucher)
		{
			var existing = await _context.Vouchers.FindAsync(voucher.VoucherId);
			if (existing == null) return null;

			_context.Entry(existing).CurrentValues.SetValues(voucher);
			await _context.SaveChangesAsync();
			return voucher;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var existing = await _context.Vouchers.FindAsync(id);
			if (existing == null) return false;

			_context.Vouchers.Remove(existing);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
