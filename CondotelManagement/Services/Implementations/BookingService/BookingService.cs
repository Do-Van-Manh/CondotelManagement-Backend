using System.Collections.Generic;
using System.Linq;
using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Services.Interfaces.BookingService;

namespace CondotelManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;

        public BookingService(IBookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public IEnumerable<BookingDTO> GetBookingsByCustomer(int customerId)
        {
            return _bookingRepo.GetBookingsByCustomerId(customerId)
                .Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    CondotelId = b.CondotelId,
                    CustomerId = b.CustomerId,
                    CheckInDate = b.StartDate,
                    CheckOutDate = b.EndDate,
                    TotalAmount = b.TotalPrice,
                    Status = b.Status
                });
        }

        public BookingDTO GetBookingById(int id)
        {
            var b = _bookingRepo.GetBookingById(id);
            if (b == null) return null;

            return new BookingDTO
            {
                BookingId = b.BookingId,
                CondotelId = b.CondotelId,
                CustomerId = b.CustomerId,
                CheckInDate = b.StartDate,
                CheckOutDate = b.EndDate,
                TotalAmount = b.TotalPrice,
                Status = b.Status
            };
        }

        public BookingDTO CreateBooking(BookingDTO dto)
        {
            var entity = new Booking
            {
                CondotelId = dto.CondotelId,
                CustomerId = dto.CustomerId,
                StartDate = dto.CheckInDate,
                EndDate = dto.CheckOutDate,
                TotalPrice = dto.TotalAmount,
                Status = "Pending"
            };

            _bookingRepo.AddBooking(entity);
            _bookingRepo.SaveChanges();

            dto.BookingId = entity.BookingId;
            return dto;
        }

        public BookingDTO UpdateBooking(BookingDTO dto)
        {
            var booking = _bookingRepo.GetBookingById(dto.BookingId);
            if (booking == null) return null;

            booking.StartDate = dto.CheckInDate;
            booking.EndDate = dto.CheckOutDate;
            booking.TotalPrice = dto.TotalAmount;
            booking.Status = dto.Status;

            _bookingRepo.UpdateBooking(booking);
            _bookingRepo.SaveChanges();

            return dto;
        }

        public bool DeleteBooking(int id)
        {
            _bookingRepo.DeleteBooking(id);
            return _bookingRepo.SaveChanges();
        }
    }
}
