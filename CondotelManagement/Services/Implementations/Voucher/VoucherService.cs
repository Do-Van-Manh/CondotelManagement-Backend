using CondotelManagement.DTOs;
using CondotelManagement.Repositories;
using CondotelManagement.Models;

namespace CondotelManagement.Services
{
	public class VoucherService : IVoucherService
	{
		private readonly IVoucherRepository _repo;

		public VoucherService(IVoucherRepository repo)
		{
			_repo = repo;
		}

		public async Task<IEnumerable<VoucherDTO>> GetVouchersByHostAsync(int hostId)
		{
			var list = await _repo.GetByHostAsync(hostId);
			return list.Select(v => new VoucherDTO
			{
				VoucherID = v.VoucherId,
				Code = v.Code,
				DiscountAmount = v.DiscountAmount,
				DiscountPercentage = v.DiscountPercentage,
				StartDate = v.StartDate,
				EndDate = v.EndDate,
				Status = v.Status
			});
		}

		public async Task<IEnumerable<VoucherDTO>> GetVouchersByCondotelAsync(int condotelId)
		{
			var list = await _repo.GetByCondotelAsync(condotelId);
			return list.Select(v => new VoucherDTO
			{
				VoucherID = v.VoucherId,
				Code = v.Code,
				DiscountAmount = v.DiscountAmount,
				DiscountPercentage = v.DiscountPercentage,
				StartDate = v.StartDate,
				EndDate = v.EndDate,
				Status = v.Status
			});
		}

		public async Task<VoucherDTO?> CreateVoucherAsync(VoucherCreateDTO dto)
		{
			var entity = new Voucher
			{
				CondotelId = dto.CondotelID,
				Code = dto.Code,
				DiscountAmount = dto.DiscountAmount,
				DiscountPercentage = dto.DiscountPercentage,
				StartDate = dto.StartDate,
				EndDate = dto.EndDate,
				UsageLimit = dto.UsageLimit,
				Status = "Active"
			};
			var saved = await _repo.AddAsync(entity);

			return new VoucherDTO
			{
				VoucherID = saved.VoucherId,
				Code = saved.Code,
				DiscountAmount = saved.DiscountAmount,
				DiscountPercentage = saved.DiscountPercentage,
				StartDate = saved.StartDate,
				EndDate = saved.EndDate,
				Status = saved.Status
			};
		}

		public async Task<VoucherDTO?> UpdateVoucherAsync(int id, VoucherCreateDTO dto)
		{
			var existing = await _repo.GetByIdAsync(id);
			if (existing == null) return null;

			existing.CondotelId = dto.CondotelID;
			existing.Code = dto.Code;
			existing.DiscountAmount = dto.DiscountAmount;
			existing.DiscountPercentage = dto.DiscountPercentage;
			existing.StartDate = dto.StartDate;
			existing.EndDate = dto.EndDate;
			existing.UsageLimit = dto.UsageLimit;

			var updated = await _repo.UpdateAsync(existing);
			if (updated == null) return null;

			return new VoucherDTO
			{
				VoucherID = updated.VoucherId,
				Code = updated.Code,
				DiscountAmount = updated.DiscountAmount,
				DiscountPercentage = updated.DiscountPercentage,
				StartDate = updated.StartDate,
				EndDate = updated.EndDate,
				Status = updated.Status
			};
		}

		public Task<bool> DeleteVoucherAsync(int id) => _repo.DeleteAsync(id);
	}
}
