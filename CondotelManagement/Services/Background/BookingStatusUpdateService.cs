using CondotelManagement.Data;
using CondotelManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CondotelManagement.Services.Background
{
    /// <summary>
    /// Background service tự động cập nhật trạng thái booking khi qua EndDate
    /// Chạy mỗi ngày lúc 00:00 UTC để cập nhật các booking đã hoàn thành
    /// Tự động tạo voucher sau khi booking completed (nếu host có cấu hình AutoGenerate)
    /// </summary>
    public class BookingStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusUpdateService> _logger;
        private const int BatchSize = 100; // Xử lý 100 bookings mỗi batch

        public BookingStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<BookingStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingStatusUpdateService is starting.");

            // Chờ đến 00:00 UTC đầu tiên
            await WaitUntilMidnight(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running scheduled booking status update at {Time}", DateTime.UtcNow);
                    await UpdateCompletedBookingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating booking statuses.");
                }

                // Chờ đến 00:00 UTC ngày hôm sau
                await WaitUntilMidnight(stoppingToken);
            }

            _logger.LogInformation("BookingStatusUpdateService is stopping.");
        }

        /// <summary>
        /// Chờ đến 00:00 UTC tiếp theo
        /// </summary>
        private async Task WaitUntilMidnight(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1); // 00:00 ngày hôm sau
            var delay = midnight - now;

            _logger.LogInformation("Waiting until {Midnight} UTC ({Delay} from now)", midnight, delay);
            await Task.Delay(delay, stoppingToken);
        }

        /// <summary>
        /// Cập nhật booking status và tự động tạo voucher
        /// Xử lý theo batch để tối ưu performance
        /// </summary>
        private async Task UpdateCompletedBookingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CondotelDbVer1Context>();
            var voucherService = scope.ServiceProvider.GetRequiredService<IVoucherService>();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                int totalProcessed = 0;
                int totalVouchersCreated = 0;

                // Lấy tổng số bookings cần xử lý
                var totalCount = await context.Bookings
                    .Where(b => b.EndDate < today && b.Status == "Confirmed")
                    .CountAsync();

                if (totalCount == 0)
                {
                    _logger.LogInformation("No bookings to update.");
                    return;
                }

                _logger.LogInformation("Found {Count} booking(s) to update. Processing in batches of {BatchSize}.", 
                    totalCount, BatchSize);

                // Xử lý theo batch
                int skip = 0;
                while (skip < totalCount)
                {
                    // Lấy batch bookings
                    var bookingsBatch = await context.Bookings
                        .Where(b => b.EndDate < today && b.Status == "Confirmed")
                        .OrderBy(b => b.BookingId) // Đảm bảo thứ tự nhất quán
                        .Skip(skip)
                        .Take(BatchSize)
                        .ToListAsync();

                    if (!bookingsBatch.Any())
                        break;

                    _logger.LogInformation("Processing batch: {Current}/{Total} bookings (Batch {BatchNumber})", 
                        skip + bookingsBatch.Count, totalCount, (skip / BatchSize) + 1);

                    // Cập nhật status và tạo voucher cho từng booking trong batch
                    foreach (var booking in bookingsBatch)
                    {
                        try
                        {
                            // 1. Cập nhật status
                            booking.Status = "Completed";
                            _logger.LogDebug(
                                "Updated booking {BookingId} status from Confirmed to Completed. EndDate: {EndDate}",
                                booking.BookingId, booking.EndDate);

                            // 2. Lưu thay đổi status trước
                            await context.SaveChangesAsync();

                            // 3. Tự động tạo voucher sau khi booking completed
                            try
                            {
                                var vouchers = await voucherService.CreateVoucherAfterBookingAsync(booking.BookingId);
                                if (vouchers != null && vouchers.Any())
                                {
                                    totalVouchersCreated += vouchers.Count;
                                    _logger.LogInformation(
                                        "Created {VoucherCount} voucher(s) for booking {BookingId} (User: {UserId})",
                                        vouchers.Count, booking.BookingId, booking.CustomerId);
                                }
                            }
                            catch (Exception voucherEx)
                            {
                                // Log lỗi nhưng không dừng quá trình update status
                                _logger.LogWarning(voucherEx,
                                    "Failed to create vouchers for booking {BookingId}. Status update succeeded.",
                                    booking.BookingId);
                            }

                            totalProcessed++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error processing booking {BookingId}. Continuing with next booking.",
                                booking.BookingId);
                            // Tiếp tục với booking tiếp theo
                        }
                    }

                    skip += BatchSize;
                }

                _logger.LogInformation(
                    "Completed processing: {Processed}/{Total} bookings updated, {VoucherCount} vouchers created.",
                    totalProcessed, totalCount, totalVouchersCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking statuses.");
                throw;
            }
        }
    }
}

