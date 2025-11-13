using CondotelManagement.DTOs;

namespace CondotelManagement.Services
{
	public interface IVoucherService
	{
		Task<IEnumerable<VoucherDTO>> GetVouchersByHostAsync(int hostId);
		Task<IEnumerable<VoucherDTO>> GetVouchersByCondotelAsync(int condotelId);
		Task<VoucherDTO?> CreateVoucherAsync(VoucherCreateDTO dto);
		Task<VoucherDTO?> UpdateVoucherAsync(int id, VoucherCreateDTO dto);
		Task<bool> DeleteVoucherAsync(int id);
		Task<List<VoucherDTO>> CreateVoucherAfterBookingAsync(VoucherAutoCreateDTO dto);
	}
}
