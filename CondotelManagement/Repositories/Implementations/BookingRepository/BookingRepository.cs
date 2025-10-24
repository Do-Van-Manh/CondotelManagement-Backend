using System.Collections.Generic;
using System.Linq;
using CondotelManagement.Data;
using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly CondotelDbVer1Context _context;

        public BookingRepository(CondotelDbVer1Context context)
        {
            _context = context;
        }

        public IEnumerable<Booking> GetBookingsByCustomerId(int customerId)
        {
            return _context.Bookings.Where(b => b.CustomerId == customerId).ToList();
        }

        public Booking GetBookingById(int id)
        {
            return _context.Bookings.FirstOrDefault(b => b.BookingId == id);
        }

        public void AddBooking(Booking booking)
        {
            _context.Bookings.Add(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            _context.Bookings.Update(booking);
        }

        public void DeleteBooking(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking != null)
                _context.Bookings.Remove(booking);
        }
        public IEnumerable<Booking> GetBookingsByRoom(int roomId)
        {
            return _context.Bookings.Where(b => b.BookingId == roomId).ToList();
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
