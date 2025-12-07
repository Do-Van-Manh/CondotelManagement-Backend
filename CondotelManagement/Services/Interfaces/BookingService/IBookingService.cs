using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Admin;
using CondotelManagement.DTOs.Booking;

namespace CondotelManagement.Services.Interfaces.BookingService
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId);
        BookingDTO GetBookingById(int id);
        Task<ServiceResultDTO> CreateBookingAsync(BookingDTO booking);

        BookingDTO UpdateBooking(BookingDTO booking);
        Task<bool> CancelBooking(int bookingId, int customerId);
        Task<bool> CancelPayment(int bookingId, int customerId);
        Task<ServiceResultDTO> RefundBooking(int bookingId, int customerId, string? bankCode = null, string? accountNumber = null, string? accountHolder = null);
        Task<ServiceResultDTO> AdminRefundBooking(int bookingId, string? reason = null);

        bool CheckAvailability(int roomId, DateOnly checkIn, DateOnly checkOut);

        IEnumerable<HostBookingDTO> GetBookingsByHost(int hostId);
        IEnumerable<HostBookingDTO> GetBookingsByHostAndCustomer(int hostId, int customerId);

        // Admin refund management
        Task<List<RefundRequestDTO>> GetRefundRequestsAsync(string? searchTerm = null, string? status = "all", DateTime? startDate = null, DateTime? endDate = null);
        Task<ServiceResultDTO> ConfirmRefundManually(int bookingId);
    }
}
