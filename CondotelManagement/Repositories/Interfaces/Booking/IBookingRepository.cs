using System.Collections.Generic;
using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetBookingsByCustomerId(int customerId);
        Booking GetBookingById(int id);
        void AddBooking(Booking booking);
        void UpdateBooking(Booking booking);


        bool SaveChanges();

        IEnumerable<Booking> GetBookingsByRoom(int roomId);
    }
}
