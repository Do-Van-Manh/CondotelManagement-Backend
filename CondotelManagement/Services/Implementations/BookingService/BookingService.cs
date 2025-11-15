using System;
using System.Collections.Generic;
using System.Linq;
using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly CondotelDbVer1Context _context = new CondotelDbVer1Context();
        private readonly IBookingRepository _bookingRepo;
        private readonly ICondotelRepository _condotelRepo; // để lấy giá phòng

        public BookingService(IBookingRepository bookingRepo, ICondotelRepository condotelRepo)
        {
            _bookingRepo = bookingRepo;
            _condotelRepo = condotelRepo;
        }

        public async Task<IEnumerable<BookingDTO>> GetBookingsByCustomerAsync(int customerId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Condotel)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.EndDate)
                .ToListAsync();

            var bookingDTOs = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                CondotelId = b.CondotelId,
                CondotelName = b.Condotel.Name,
                CustomerId = b.CustomerId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                PromotionId = b.PromotionId,
                CreatedAt = b.CreatedAt,

                // Logic hiển thị nút review
                CanReview = b.Status == "Completed"
                         && b.EndDate < DateOnly.FromDateTime(DateTime.Now)
                         && !_context.Reviews.Any(r => r.BookingId == b.BookingId),

                HasReviewed = _context.Reviews.Any(r => r.BookingId == b.BookingId)
            }).ToList();

            return bookingDTOs;
        }

        public BookingDTO GetBookingById(int id)
        {
            var b = _bookingRepo.GetBookingById(id);
            return b == null ? null : ToDTO(b);
        }

        public bool CheckAvailability(int condotelId, DateOnly checkIn, DateOnly checkOut)
        {
            var bookings = _bookingRepo.GetBookingsByCondotel(condotelId);
            return !bookings.Any(b =>
                b.Status != "Cancelled" && (checkIn < b.EndDate && checkOut > b.StartDate)
            );
        }

        public BookingDTO CreateBooking(BookingDTO dto)
        {
            // Kiểm tra trống
            if (!CheckAvailability(dto.CondotelId, dto.StartDate, dto.EndDate))
                throw new InvalidOperationException("Condotel is not available in this period.");

            // Lấy giá phòng
            var condotel = _condotelRepo.GetCondotelById(dto.CondotelId);
            if (condotel == null)
                throw new InvalidOperationException("Condotel not found.");

            // Tính tổng tiền
            int days = (dto.EndDate.ToDateTime(TimeOnly.MinValue) - dto.StartDate.ToDateTime(TimeOnly.MinValue)).Days;
            if (days <= 0) throw new InvalidOperationException("Invalid date range.");

            decimal price = condotel.PricePerNight * days;

            // Giảm giá khuyến mãi (nếu có)
            if (dto.PromotionId.HasValue)
            {
                var promo = _condotelRepo.GetPromotionById(dto.PromotionId.Value);
                if (promo != null)
                    price -= price * (promo.DiscountPercentage / 100m);
            }
            if (dto.PromotionId == 0)
            {
                dto.PromotionId = null;
            }


            dto.TotalPrice = price;
            dto.Status = "Pending";
            dto.CreatedAt = DateTime.Now;

            var entity = ToEntity(dto);
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
            booking.Status = dto.Status;
            booking.TotalPrice = dto.TotalPrice;

            _bookingRepo.UpdateBooking(booking);
            _bookingRepo.SaveChanges();

            return ToDTO(booking);
        }

      
         public bool CancelBooking(int bookingId, int customerId)
        {
            var booking = _bookingRepo.GetBookingById(bookingId);
            if (booking == null || booking.CustomerId != customerId)
                return false;

            booking.Status = "Cancelled";
            _bookingRepo.UpdateBooking(booking);
            return _bookingRepo.SaveChanges();
        }
        // Helper mapping
        private BookingDTO ToDTO(Booking b) => new BookingDTO
        {
            BookingId = b.BookingId,
            CondotelId = b.CondotelId,
            CustomerId = b.CustomerId,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            PromotionId = b.PromotionId,
            CreatedAt = b.CreatedAt
        };

        private Booking ToEntity(BookingDTO dto) => new Booking
        {
            BookingId = dto.BookingId,
            CondotelId = dto.CondotelId,
            CustomerId = dto.CustomerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalPrice = dto.TotalPrice,
            Status = dto.Status,
            PromotionId = dto.PromotionId,
            CreatedAt = dto.CreatedAt
        };

        public IEnumerable<HostBookingDTO> GetBookingsByHost(int hostId)
        {
            return _bookingRepo.GetBookingsByHost(hostId);
        }

        public IEnumerable<HostBookingDTO> GetBookingsByHostAndCustomer(int hostId, int customerId)
        {
            return _bookingRepo.GetBookingsByHostAndCustomer(hostId, customerId);
        }
    }
}
