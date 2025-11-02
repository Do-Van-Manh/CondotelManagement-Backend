using System.Collections.Generic;
using CondotelManagement.DTOs;
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
        IEnumerable<HostBookingDTO> GetBookingsByHost(int hostId);
        IEnumerable<HostBookingDTO> GetBookingsByHostAndCustomer(int hostId, int customerId);
    }
}
