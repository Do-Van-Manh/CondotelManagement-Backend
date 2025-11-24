using CondotelManagement.Data;
using CondotelManagement.DTOs.Package;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CondotelManagement.Repositories.Interfaces.Admin;
using System.Text.RegularExpressions;
using CondotelManagement.Services.Interfaces.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CondotelManagement.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly CondotelDbVer1Context _context;
        private readonly IPackageFeatureService _featureService;
        private readonly IUserRepository _userRepo;

        public PackageService(CondotelDbVer1Context context, IPackageFeatureService featureService, IUserRepository userRepo)
        {
            _context = context;
            _featureService = featureService;
            _userRepo = userRepo;
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
                Duration = p.Duration, // Giữ string Duration
                Description = p.Description,
                MaxListings = _featureService.GetMaxListingCount(p.PackageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(p.PackageId)
            });
        }

        public async Task<HostPackageDetailsDto?> GetMyActivePackageAsync(int hostId)
        {
            // SỬA: Chuyển DateTime.UtcNow sang DateOnly để so sánh
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var activePackage = await _context.HostPackages
                .Include(hp => hp.Package)
                .Where(hp => hp.HostId == hostId &&
                             hp.Status == "Active" &&
                             // SỬA: So sanh DateOnly >= DateOnly
                             hp.EndDate >= today)
                .FirstOrDefaultAsync();

            if (activePackage == null) return null;

            var currentListings = await _context.Condotels
                .CountAsync(c => c.HostId == hostId && c.Status != "Deleted");

            return MapToDetailsDto(activePackage, currentListings);
        }

        public async Task<HostPackageDetailsDto> PurchaseOrUpgradePackageAsync(int hostId, int packageId)
        {
            var packageToBuy = await _context.Packages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.Status == "Active");

            if (packageToBuy == null)
                throw new Exception("Gói dịch vụ không hợp lệ hoặc đã ngừng hoạt động.");

            var durationDays = ParseDuration(packageToBuy.Duration);

            // XÓA GÓI CŨ
            var oldPackage = await _context.HostPackages.FirstOrDefaultAsync(hp => hp.HostId == hostId);
            if (oldPackage != null)
                _context.HostPackages.Remove(oldPackage);

            // TẠO ORDERCODE TRƯỚC (để tránh lỗi null)
            var randomPart = new Random().Next(100000, 999999);
            var orderCode = (long)hostId * 1_000_000_000L + (long)packageId * 1_000_000L + randomPart;

            while (orderCode > 99999999999999L)
            {
                randomPart = new Random().Next(100000, 999999);
                orderCode = (long)hostId * 1_000_000_000L + (long)packageId * 1_000_000L + randomPart;
            }

            // TẠO BẢN GHI MỚI – GÁN GIÁ TRỊ MẶC ĐỊNH CHO DateOnly (non-nullable)
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var futureDate = today.AddYears(100); // ngày xa xỉ để tạm = "chưa kích hoạt"

            var newHostPackage = new HostPackage
            {
                HostId = hostId,
                PackageId = packageId,
                Status = "PendingPayment",
                DurationDays = durationDays,
                OrderCode = orderCode.ToString(),
                StartDate = futureDate,  // GÁN GIÁ TRỊ MẶC ĐỊNH ĐỂ TRÁNH LỖI
                EndDate = futureDate     // GÁN GIÁ TRỊ MẶC ĐỊNH
            };

            _context.HostPackages.Add(newHostPackage);
            await _context.SaveChangesAsync();

            // Cập nhật lại thành NULL (hoặc để trống) SAU khi đã Add
            newHostPackage.StartDate = null;  // BÂY GIỜ MỚI GÁN NULL ĐƯỢC (vì đã qua EF validation)
            newHostPackage.EndDate = null;
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
                Message = "Đã tạo đơn hàng thành công! Đang chuyển đến cổng thanh toán PayOS...",
                PaymentUrl = null,
                OrderCode = orderCode,
                Amount = packageToBuy.Price.GetValueOrDefault(0)
            };
        }

        // Ham helper de parse "30 days", "90 days"
        private int ParseDuration(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;
            var match = Regex.Match(duration, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int days))
            {
                return days;
            }
            return 30; // Mac dinh 30 ngay
        }

        // Ham helper de map DTO
        private HostPackageDetailsDto MapToDetailsDto(HostPackage hostPackage, int currentListings)
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
                Message = null,
                PaymentUrl = null,
                OrderCode = 0,
                Amount = 0
            };
        }
    }
}