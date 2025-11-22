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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var durationDays = ParseDuration(packageToBuy.Duration);

            // TẠO ORDERCODE AN TOÀN 100% CHO PAYOS – 14 CHỮ SỐ – ĐẸP NHƯ MÃ VẬN ĐƠN!
            var orderCode = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            // Ví dụ: 20251122153045 → 14 chữ số → PayOS yêu thương liền!

            // XÓA GÓI CŨ (nếu có)
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM HostPackage WHERE HostID = {0}", hostId);

            // TẠO GÓI MỚI – STATUS = PendingPayment, CHƯA CÓ STARTDATE & ENDDATE
            await _context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO HostPackage (HostID, PackageID, StartDate, EndDate, Status, OrderCode, DurationDays)
          VALUES ({0}, {1}, NULL, NULL, 'PendingPayment', {2}, {3})",
                hostId, packageId, orderCode, durationDays);

            var currentListings = await _context.Condotels
                .CountAsync(c => c.HostId == hostId && c.Status != "Deleted");

            return new HostPackageDetailsDto
            {
                PackageName = packageToBuy.Name,
                Status = "PendingPayment", // Đang chờ thanh toán
                StartDate = null,          // Chưa có (webhook sẽ fill khi thành công)
                EndDate = null,            // Chưa có
                CurrentListings = currentListings,
                MaxListings = _featureService.GetMaxListingCount(packageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(packageId),
                Message = "Đã tạo đơn hàng thành công! Đang chuyển đến cổng thanh toán PayOS...",
                PaymentUrl = null,
                OrderCode = orderCode,     // ← Quan trọng: trả về cho FE
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
                StartDate = hostPackage.StartDate.ToString("yyyy-MM-dd"),  // ← ĐÃ FIX
                EndDate = hostPackage.EndDate.ToString("yyyy-MM-dd"),      // ← ĐÃ FIX
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