using CondotelManagement.DTOs.Host;
// SỬA: Bỏ alias và dùng tên đầy đủ để tránh nhầm lẫn
// using HostModel = CondotelManagement.Models.Host; 

namespace CondotelManagement.Services.Interfaces // Sửa: Thêm Interfaces
{
    public interface IHostService
    {
        // SỬA: Dùng tên đầy đủ
        CondotelManagement.Models.Host GetByUserId(int userId);

        Task<bool> CanHostUploadCondotel(int hostId);

        // SỬA: Dùng tên đầy đủ
        Task<HostRegistrationResponseDto> RegisterHostAsync(int userId, HostRegisterRequestDto dto);
    }
}