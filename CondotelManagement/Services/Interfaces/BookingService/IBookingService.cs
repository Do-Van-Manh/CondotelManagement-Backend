using CondotelManagement.DTOs;

namespace CondotelManagement.Services.Interfaces.BookingService
{
    public interface IBookingService
    {
        IEnumerable<BookingDTO> GetBookingsByCustomer(int customerId);
        BookingDTO GetBookingById(int id);
        BookingDTO CreateBooking(BookingDTO booking);
        BookingDTO UpdateBooking(BookingDTO booking);
        bool CancelBooking(int id);
        bool CheckAvailability(int roomId, DateOnly checkIn, DateOnly checkOut);

    }
}
