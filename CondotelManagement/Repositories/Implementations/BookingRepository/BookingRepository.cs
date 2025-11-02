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
            => _context.Bookings.Where(b => b.CustomerId == customerId).ToList();

        public Booking GetBookingById(int id)
            => _context.Bookings.FirstOrDefault(b => b.BookingId == id);

        public IEnumerable<Booking> GetBookingsByCondotel(int condotelId)
            => _context.Bookings.Where(b => b.CondotelId == condotelId).ToList();

        public void AddBooking(Booking booking)
        {
            _context.Bookings.Add(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            _context.Bookings.Update(booking);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
