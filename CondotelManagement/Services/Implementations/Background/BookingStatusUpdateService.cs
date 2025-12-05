using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CondotelManagement.Data;
using CondotelManagement.Services.Interfaces.Shared;

namespace CondotelManagement.Services.Background
{
    public class BookingStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusUpdateService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

        public BookingStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<BookingStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[BookingStatusUpdate] Service is starting...");

            // Chờ 10 giây để app khởi động hoàn tất
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"[BookingStatusUpdate] Running scheduled check (Interval: {_interval.TotalMinutes} minutes)");
                    await UpdateExpiredBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BookingStatusUpdate] Error occurred while updating booking statuses");
                }

                // Đợi đến lần chạy tiếp theo
                _logger.LogInformation($"[BookingStatusUpdate] Next check in {_interval.TotalMinutes} minutes");

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("[BookingStatusUpdate] Service cancellation requested");
                    break;
                }
            }

            _logger.LogInformation("[BookingStatusUpdate] Service is stopping...");
        }

        private async Task UpdateExpiredBookingsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[BookingStatusUpdate] Checking for expired bookings...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CondotelDbVer1Context>();
   

            var today = DateOnly.FromDateTime(DateTime.Today);

            var expiredBookings = await context.Bookings
                .Where(b => b.Status == "Confirmed" && b.EndDate < today)
                .ToListAsync(cancellationToken);

            if (!expiredBookings.Any())
            {
                _logger.LogInformation("[BookingStatusUpdate] No expired bookings found.");
                return;
            }

            _logger.LogInformation($"[BookingStatusUpdate] Found {expiredBookings.Count} expired booking(s) to update.");

            var updatedCount = 0;
            var failedCount = 0;

            foreach (var booking in expiredBookings)
            {
                try
                {
                    _logger.LogInformation($"[BookingStatusUpdate] Processing booking #{booking.BookingId} (EndDate: {booking.EndDate})");

                    booking.Status = "Completed";
                 

                    updatedCount++;
                    _logger.LogInformation($"[BookingStatusUpdate] Successfully updated booking #{booking.BookingId} to Completed");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, $"[BookingStatusUpdate] Failed to update booking #{booking.BookingId}");
                }
            }

            try
            {
                var savedChanges = await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"[BookingStatusUpdate] SaveChanges completed. Rows affected: {savedChanges}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BookingStatusUpdate] Failed to save changes to database");
                throw;
            }

            _logger.LogInformation(
                $"[BookingStatusUpdate] Update completed. Success: {updatedCount}, Failed: {failedCount}"
            );
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 BookingStatusUpdateService is stopping gracefully...");
            await base.StopAsync(cancellationToken);
        }
    }
}
