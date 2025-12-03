using CondotelManagement.Data;
using CondotelManagement.DTOs.Host;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces.Host;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Services.Implementations.Host
{
    public class HostPayoutService : IHostPayoutService
    {
        private readonly CondotelDbVer1Context _context;

        public HostPayoutService(CondotelDbVer1Context context)
        {
            _context = context;
        }

        public async Task<HostPayoutResponseDTO> ProcessPayoutsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var cutoffDate = today.AddDays(-15); // 15 ngày trước

            // Lấy các booking đã completed >= 15 ngày, chưa được trả tiền
            var eligibleBookings = await _context.Bookings
                .Include(b => b.Condotel)
                    .ThenInclude(c => c.Host)
                .Where(b => b.Status == "Completed"
                    && b.EndDate <= cutoffDate
                    && !b.IsPaidToHost
                    && b.TotalPrice.HasValue
                    && b.TotalPrice.Value > 0)
                .ToListAsync();

            // Kiểm tra xem có refund request nào không
            var bookingIds = eligibleBookings.Select(b => b.BookingId).ToList();
            var refundRequests = await _context.RefundRequests
                .Where(r => bookingIds.Contains(r.BookingId) 
                    && (r.Status == "Pending" || r.Status == "Approved"))
                .Select(r => r.BookingId)
                .ToListAsync();

            // Loại bỏ các booking có refund request
            var bookingsToProcess = eligibleBookings
                .Where(b => !refundRequests.Contains(b.BookingId))
                .ToList();

            var processedItems = new List<HostPayoutItemDTO>();
            decimal totalAmount = 0;

            foreach (var booking in bookingsToProcess)
            {
                // Đánh dấu đã trả tiền
                booking.IsPaidToHost = true;
                booking.PaidToHostAt = DateTime.UtcNow;

                var daysSinceCompleted = (today.ToDateTime(TimeOnly.MinValue) - booking.EndDate.ToDateTime(TimeOnly.MinValue)).Days;

                processedItems.Add(new HostPayoutItemDTO
                {
                    BookingId = booking.BookingId,
                    CondotelId = booking.CondotelId,
                    CondotelName = booking.Condotel.Name,
                    HostId = booking.Condotel.HostId,
                    HostName = booking.Condotel.Host?.CompanyName ?? "N/A",
                    Amount = booking.TotalPrice ?? 0m,
                    EndDate = booking.EndDate,
                    PaidAt = booking.PaidToHostAt,
                    IsPaid = true,
                    DaysSinceCompleted = daysSinceCompleted
                });

                totalAmount += booking.TotalPrice ?? 0m;
            }

            if (processedItems.Any())
            {
                await _context.SaveChangesAsync();
            }

            return new HostPayoutResponseDTO
            {
                Success = true,
                Message = $"Đã xử lý {processedItems.Count} booking và trả {totalAmount:N0} VNĐ cho host.",
                ProcessedCount = processedItems.Count,
                TotalAmount = totalAmount,
                ProcessedItems = processedItems
            };
        }

        public async Task<HostPayoutResponseDTO> ProcessPayoutForBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Condotel)
                    .ThenInclude(c => c.Host)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return new HostPayoutResponseDTO
                {
                    Success = false,
                    Message = "Booking not found."
                };
            }

            if (booking.Status != "Completed")
            {
                return new HostPayoutResponseDTO
                {
                    Success = false,
                    Message = "Booking must be completed to process payout."
                };
            }

            if (booking.IsPaidToHost)
            {
                return new HostPayoutResponseDTO
                {
                    Success = false,
                    Message = "Booking has already been paid to host."
                };
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var cutoffDate = today.AddDays(-15);

            if (booking.EndDate > cutoffDate)
            {
                return new HostPayoutResponseDTO
                {
                    Success = false,
                    Message = $"Booking must be completed for at least 15 days. EndDate: {booking.EndDate:yyyy-MM-dd}, Required: {cutoffDate:yyyy-MM-dd}"
                };
            }

            // Kiểm tra có refund request không
            var hasRefundRequest = await _context.RefundRequests
                .AnyAsync(r => r.BookingId == bookingId 
                    && (r.Status == "Pending" || r.Status == "Approved"));

            if (hasRefundRequest)
            {
                return new HostPayoutResponseDTO
                {
                    Success = false,
                    Message = "Cannot process payout. Booking has pending or approved refund request."
                };
            }

            // Đánh dấu đã trả tiền
            booking.IsPaidToHost = true;
            booking.PaidToHostAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var daysSinceCompleted = (today.ToDateTime(TimeOnly.MinValue) - booking.EndDate.ToDateTime(TimeOnly.MinValue)).Days;

            return new HostPayoutResponseDTO
            {
                Success = true,
                Message = $"Đã trả {booking.TotalPrice:N0} VNĐ cho host.",
                ProcessedCount = 1,
                TotalAmount = booking.TotalPrice ?? 0m,
                ProcessedItems = new List<HostPayoutItemDTO>
                {
                    new HostPayoutItemDTO
                    {
                        BookingId = booking.BookingId,
                        CondotelId = booking.CondotelId,
                        CondotelName = booking.Condotel.Name,
                        HostId = booking.Condotel.HostId,
                        HostName = booking.Condotel.Host?.CompanyName ?? "N/A",
                        Amount = booking.TotalPrice ?? 0m,
                        EndDate = booking.EndDate,
                        PaidAt = booking.PaidToHostAt,
                        IsPaid = true,
                        DaysSinceCompleted = daysSinceCompleted
                    }
                }
            };
        }

        public async Task<List<HostPayoutItemDTO>> GetPendingPayoutsAsync(int? hostId = null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var cutoffDate = today.AddDays(-15);

            var query = _context.Bookings
                .Include(b => b.Condotel)
                    .ThenInclude(c => c.Host)
                .Where(b => b.Status == "Completed"
                    && b.EndDate <= cutoffDate
                    && !b.IsPaidToHost
                    && b.TotalPrice.HasValue
                    && b.TotalPrice.Value > 0);

            if (hostId.HasValue)
            {
                query = query.Where(b => b.Condotel.HostId == hostId.Value);
            }

            var bookings = await query.ToListAsync();

            // Lấy danh sách booking có refund request
            var bookingIds = bookings.Select(b => b.BookingId).ToList();
            var refundRequests = await _context.RefundRequests
                .Where(r => bookingIds.Contains(r.BookingId)
                    && (r.Status == "Pending" || r.Status == "Approved"))
                .Select(r => r.BookingId)
                .ToListAsync();

            // Loại bỏ các booking có refund request
            var eligibleBookings = bookings
                .Where(b => !refundRequests.Contains(b.BookingId))
                .ToList();

            return eligibleBookings.Select(b =>
            {
                var daysSinceCompleted = (today.ToDateTime(TimeOnly.MinValue) - b.EndDate.ToDateTime(TimeOnly.MinValue)).Days;
                return new HostPayoutItemDTO
                {
                    BookingId = b.BookingId,
                    CondotelId = b.CondotelId,
                    CondotelName = b.Condotel.Name,
                    HostId = b.Condotel.HostId,
                    HostName = b.Condotel.Host?.CompanyName ?? "N/A",
                    Amount = b.TotalPrice ?? 0m,
                    EndDate = b.EndDate,
                    PaidAt = null,
                    IsPaid = false,
                    DaysSinceCompleted = daysSinceCompleted
                };
            }).ToList();
        }
    }
}

