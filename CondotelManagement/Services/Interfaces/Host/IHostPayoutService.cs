using CondotelManagement.DTOs.Host;

namespace CondotelManagement.Services.Interfaces.Host
{
    public interface IHostPayoutService
    {
        Task<HostPayoutResponseDTO> ProcessPayoutsAsync();
        Task<HostPayoutResponseDTO> ProcessPayoutForBookingAsync(int bookingId);
        Task<List<HostPayoutItemDTO>> GetPendingPayoutsAsync(int? hostId = null);
        Task<List<HostPayoutItemDTO>> GetPaidPayoutsAsync(int? hostId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}


