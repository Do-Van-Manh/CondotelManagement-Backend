using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Booking;

namespace CondotelManagement.Services.Interfaces.BookingService
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId);
        BookingDTO GetBookingById(int id);
        ServiceResultDTO CreateBooking(BookingDTO booking);

        BookingDTO UpdateBooking(BookingDTO booking);
        bool CancelBooking(int bookingId, int customerId);

        bool CheckAvailability(int roomId, DateOnly checkIn, DateOnly checkOut);

        IEnumerable<HostBookingDTO> GetBookingsByHost(int hostId);
        IEnumerable<HostBookingDTO> GetBookingsByHostAndCustomer(int hostId, int customerId);
    }
}
