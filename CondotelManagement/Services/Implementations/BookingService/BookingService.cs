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
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalPrice = b.TotalPrice,
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
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            };
        }

        public BookingDTO CreateBooking(BookingDTO dto)
        {
            var entity = new Booking
            {
                CondotelId = dto.CondotelId,
                CustomerId = dto.CustomerId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalPrice = dto.TotalPrice,
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

            booking.StartDate = dto.StartDate;
            booking.EndDate = dto.EndDate;
            booking.TotalPrice = dto.TotalPrice;
            booking.Status = dto.Status;

            _bookingRepo.UpdateBooking(booking);
            _bookingRepo.SaveChanges();

            return dto;
        }


        public bool CancelBooking(int id)
        {
             var booking = _bookingRepo.GetBookingById(id);
            if (booking == null) return false;

            booking.Status = "Cancelled";
            _bookingRepo.UpdateBooking(booking);
            return _bookingRepo.SaveChanges();
        }

        public bool CheckAvailability(int roomId, DateOnly checkIn, DateOnly checkOut)
        {
            var bookings = _bookingRepo.GetBookingsByRoom(roomId);

            // Kiểm tra xem có khoảng nào bị trùng ngày
            bool isAvailable = !bookings.Any(b =>
                           (checkIn < b.EndDate && checkOut > b.StartDate)
                                      );

            return isAvailable;
        }
    }
}
