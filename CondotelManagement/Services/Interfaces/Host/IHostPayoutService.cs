using CondotelManagement.DTOs.Host;

namespace CondotelManagement.Services.Interfaces.Host
{
    public interface IHostPayoutService
    {
        Task<HostPayoutResponseDTO> ProcessPayoutsAsync();
        Task<HostPayoutResponseDTO> ProcessPayoutForBookingAsync(int bookingId);
        Task<List<HostPayoutItemDTO>> GetPendingPayoutsAsync(int? hostId = null);
    }
}

