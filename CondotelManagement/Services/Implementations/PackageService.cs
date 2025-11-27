using CondotelManagement.Data;
using CondotelManagement.DTOs.Package;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CondotelManagement.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly CondotelDbVer1Context _context;
        private readonly IPackageFeatureService _featureService;

        public PackageService(CondotelDbVer1Context context, IPackageFeatureService featureService)
        {
            _context = context;
            _featureService = featureService;
        }

        public async Task<IEnumerable<PackageDto>> GetAvailablePackagesAsync()
        {
            var packages = await _context.Packages
                .Where(p => p.Status == "Active")
                .ToListAsync();

            return packages.Select(p => new PackageDto
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Price = p.Price.GetValueOrDefault(0),
                Duration = p.Duration ?? "30 days",
                Description = p.Description ?? "",
                MaxListings = _featureService.GetMaxListingCount(p.PackageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(p.PackageId),
                MaxBlogRequestsPerMonth = _featureService.GetMaxBlogRequestsPerMonth(p.PackageId),
                IsVerifiedBadgeEnabled = _featureService.IsVerifiedBadgeEnabled(p.PackageId),
                DisplayColorTheme = _featureService.GetDisplayColorTheme(p.PackageId),
                PriorityLevel = _featureService.GetPriorityLevel(p.PackageId)
            });
        }

        public async Task<HostPackageDetailsDto?> GetMyActivePackageAsync(int hostId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // SỬA: Tìm HostPackage active, không cần composite key
            var activePackage = await _context.HostPackages
                .Include(hp => hp.Package)
                .Where(hp => hp.HostId == hostId &&
                             hp.Status == "Active" &&
                             hp.EndDate.HasValue &&
                             hp.EndDate.Value >= today)
                .OrderByDescending(hp => hp.EndDate) // Lấy gói mới nhất
                .FirstOrDefaultAsync();

            if (activePackage == null) return null;

            var currentListings = await _context.Condotels
                .CountAsync(c => c.HostId == hostId && c.Status != "Deleted");

            // Đếm số blog request trong tháng
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var usedBlogRequests = await _context.BlogRequests
                .CountAsync(br => br.HostId == hostId && br.RequestDate >= startOfMonth);

            return MapToDetailsDto(activePackage, currentListings, usedBlogRequests);
        }

        public async Task<HostPackageDetailsDto> PurchaseOrUpgradePackageAsync(int hostId, int packageId)
        {
            var packageToBuy = await _context.Packages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.Status == "Active");

            if (packageToBuy == null)
                throw new Exception("Gói dịch vụ không hợp lệ hoặc đã ngừng hoạt động.");

            var durationDays = ParseDuration(packageToBuy.Duration ?? "30 days");

            // SỬA: Đánh dấu tất cả gói cũ là Inactive thay vì xóa
            var oldPackages = await _context.HostPackages
                .Where(hp => hp.HostId == hostId && hp.Status == "Active")
                .ToListAsync();

            foreach (var old in oldPackages)
            {
                old.Status = "Inactive";
            }

            // TẠO ORDERCODE
            var randomPart = new Random().Next(100000, 999999);
            var orderCode = (long)hostId * 1_000_000_000L + (long)packageId * 1_000_000L + randomPart;

            while (orderCode > 99999999999999L)
            {
                randomPart = new Random().Next(100000, 999999);
                orderCode = (long)hostId * 1_000_000_000L + (long)packageId * 1_000_000L + randomPart;
            }

            // TẠO BẢN GHI MỚI với HostPackageId tự động tăng
            var newHostPackage = new HostPackage
            {
                HostId = hostId,
                PackageId = packageId,
                Status = "PendingPayment",
                DurationDays = durationDays,
                OrderCode = orderCode.ToString(),
                StartDate = null,  // Sẽ được cập nhật sau khi thanh toán thành công
                EndDate = null
            };

            _context.HostPackages.Add(newHostPackage);
            await _context.SaveChangesAsync();

            var currentListings = await _context.Condotels
                .CountAsync(c => c.HostId == hostId && c.Status != "Deleted");

            return new HostPackageDetailsDto
            {
                PackageName = packageToBuy.Name,
                Status = "PendingPayment",
                StartDate = null,
                EndDate = null,
                CurrentListings = currentListings,
                MaxListings = _featureService.GetMaxListingCount(packageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(packageId),
                MaxBlogRequestsPerMonth = _featureService.GetMaxBlogRequestsPerMonth(packageId),
                UsedBlogRequestsThisMonth = 0,
                IsVerifiedBadgeEnabled = _featureService.IsVerifiedBadgeEnabled(packageId),
                DisplayColorTheme = _featureService.GetDisplayColorTheme(packageId),
                PriorityLevel = _featureService.GetPriorityLevel(packageId),
                Message = "Đã tạo đơn hàng thành công! Đang chuyển đến cổng thanh toán PayOS...",
                PaymentUrl = null,
                OrderCode = orderCode,
                Amount = packageToBuy.Price.GetValueOrDefault(0)
            };
        }

        private int ParseDuration(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 30;
            var match = Regex.Match(duration, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int days))
            {
                return days;
            }
            return 30;
        }

        private HostPackageDetailsDto MapToDetailsDto(HostPackage hostPackage, int currentListings, int usedBlogRequests)
        {
            return new HostPackageDetailsDto
            {
                PackageName = hostPackage.Package?.Name ?? "Không xác định",
                Status = hostPackage.Status,
                StartDate = hostPackage.StartDate?.ToString("yyyy-MM-dd"),
                EndDate = hostPackage.EndDate?.ToString("yyyy-MM-dd"),
                CurrentListings = currentListings,
                MaxListings = _featureService.GetMaxListingCount(hostPackage.PackageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(hostPackage.PackageId),
                MaxBlogRequestsPerMonth = _featureService.GetMaxBlogRequestsPerMonth(hostPackage.PackageId),
                UsedBlogRequestsThisMonth = usedBlogRequests,
                IsVerifiedBadgeEnabled = _featureService.IsVerifiedBadgeEnabled(hostPackage.PackageId),
                DisplayColorTheme = _featureService.GetDisplayColorTheme(hostPackage.PackageId),
                PriorityLevel = _featureService.GetPriorityLevel(hostPackage.PackageId),
                Message = null,
                PaymentUrl = null,
                OrderCode = 0,
                Amount = 0
            };
        }
    }
}