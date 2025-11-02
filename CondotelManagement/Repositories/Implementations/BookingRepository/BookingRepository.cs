using System.Collections.Generic;
using System.Linq;
using CondotelManagement.Data;
using CondotelManagement.DTOs;
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

        public IEnumerable<HostBookingDTO> GetBookingsByHost(int hostId)
        {
            return _context.Bookings
            .Where(b => b.Condotel.HostId == hostId)
            .Select(b => new HostBookingDTO
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer.FullName,
                CustomerPhone = b.Customer.Phone,
                CustomerEmail = b.Customer.Email,
                CondotelName = b.Condotel.Name,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,

                Services = b.BookingDetails
                    .Select(d => new BookingServiceDTO
                    {
                        ServiceName = d.Service.Name,
                        Quantity = d.Quantity,
                        Price = d.Price
                    }).ToList()
            })
            .OrderByDescending(x => x.StartDate)
            .ToList();
        }

        public IEnumerable<HostBookingDTO> GetBookingsByHostAndCustomer(int hostId, int customerId)
        {
            return _context.Bookings
            .Where(b => b.Condotel.HostId == hostId && b.Customer.UserId == customerId)
            .Select(b => new HostBookingDTO
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer.FullName,
                CustomerPhone = b.Customer.Phone,
                CustomerEmail = b.Customer.Email,
                CondotelName = b.Condotel.Name,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,

                Services = b.BookingDetails
                    .Select(d => new BookingServiceDTO
                    {
                        ServiceName = d.Service.Name,
                        Quantity = d.Quantity,
                        Price = d.Price
                    }).ToList()
            })
            .OrderByDescending(x => x.StartDate)
            .ToList();
        }
    }
}
