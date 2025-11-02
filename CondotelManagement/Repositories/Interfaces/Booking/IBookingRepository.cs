using System.Collections.Generic;
using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetBookingsByCustomerId(int customerId);
        Booking GetBookingById(int id);
        IEnumerable<Booking> GetBookingsByCondotel(int condotelId);
        void AddBooking(Booking booking);
        void UpdateBooking(Booking booking);
        bool SaveChanges();
    }
}
