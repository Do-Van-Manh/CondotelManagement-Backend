using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Voucher;

namespace CondotelManagement.Services
{
	public interface IVoucherService
	{
		Task<IEnumerable<VoucherDTO>> GetVouchersByHostAsync(int hostId);
		Task<IEnumerable<VoucherDTO>> GetVouchersByCondotelAsync(int condotelId);
		Task<VoucherDTO?> CreateVoucherAsync(VoucherCreateDTO dto);
		Task<VoucherDTO?> UpdateVoucherAsync(int id, VoucherCreateDTO dto);
		Task<bool> DeleteVoucherAsync(int id);
		Task<List<VoucherDTO>> CreateVoucherAfterBookingAsync(int bookingId);
		Task<HostVoucherSettingDetailDTO?> GetSettingAsync(int hostId);
		Task<HostVoucherSettingDetailDTO> SaveSettingAsync(int hostId, HostVoucherSettingDTO dto);
	}
}
