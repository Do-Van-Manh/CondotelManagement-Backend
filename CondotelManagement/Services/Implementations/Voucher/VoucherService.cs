using CondotelManagement.DTOs;
using CondotelManagement.Repositories;
using CondotelManagement.Models;
using Microsoft.EntityFrameworkCore;

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
				CondotelID = v.Condotel?.CondotelId,
				CondotelName = v.Condotel?.Name,
				UserID = v.User?.UserId,
				FullName = v.User?.FullName,
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
				CondotelID = v.Condotel?.CondotelId,
				CondotelName = v.Condotel?.Name,
				UserID = v.User?.UserId,
				FullName = v.User?.FullName,
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
				UserId = dto.UserID,
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
				CondotelID = saved.Condotel.CondotelId,
				CondotelName = saved.Condotel.Name,
				UserID = saved.User?.UserId,
				FullName = saved.User?.FullName,
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
			existing.UserId = dto.UserID;
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
				CondotelID = updated.Condotel.CondotelId,
				CondotelName = updated.Condotel.Name,
				UserID = updated.User?.UserId,
				FullName = updated.User?.FullName,
				Code = updated.Code,
				DiscountAmount = updated.DiscountAmount,
				DiscountPercentage = updated.DiscountPercentage,
				StartDate = updated.StartDate,
				EndDate = updated.EndDate,
				Status = updated.Status
			};
		}

		public Task<bool> DeleteVoucherAsync(int id) => _repo.DeleteAsync(id);

		public async Task<VoucherDTO?> CreateVoucherAfterBookingAsync(VoucherAutoCreateDTO dto)
		{
			// Sinh code tự động nếu không có
			string code = await _repo.GenerateUniqueVoucherCodeAsync(dto.UserID);

			var entity = new Voucher
			{
				CondotelId = dto.CondotelID,
				UserId = dto.UserID,
				Code = code,
				DiscountAmount = 200000,            // fixed giảm 200k
				DiscountPercentage = 10,             // 10% giảm
				StartDate = DateOnly.FromDateTime(DateTime.Today),       // Chuyển sang DateOnly
				EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)), // Chuyển sang DateOnly
				UsageLimit = 1,
				Status = "Active"
			};
			var saved = await _repo.AddAsync(entity);

			return new VoucherDTO
			{
				VoucherID = saved.VoucherId,
				CondotelID = saved.Condotel.CondotelId,
				CondotelName = saved.Condotel.Name,
				UserID = saved.User?.UserId,
				FullName = saved.User?.FullName,
				Code = saved.Code,
				DiscountAmount = saved.DiscountAmount,
				DiscountPercentage = saved.DiscountPercentage,
				StartDate = saved.StartDate,
				EndDate = saved.EndDate,
				Status = saved.Status
			};
		}


	}
}
