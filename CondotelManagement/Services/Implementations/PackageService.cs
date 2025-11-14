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
            var packageToBuy = await _context.Packages.FindAsync(packageId);
            if (packageToBuy == null || packageToBuy.Status != "Active")
            {
                throw new Exception("Gói dịch vụ không hợp lệ.");
            }

            // (LOGIC THANH TOAN DAT O DAY)

            // 2. Huy goi cu
            var oldPackages = await _context.HostPackages
                .Where(hp => hp.HostId == hostId && hp.Status == "Active")
                .ToListAsync();

            foreach (var oldPkg in oldPackages)
            {
                oldPkg.Status = "Upgraded";
            }

            // 3. Tao goi moi
            var today = DateOnly.FromDateTime(DateTime.UtcNow); // Lấy ngày hôm nay
            var newHostPackage = new HostPackage
            {
                HostId = hostId,
                PackageId = packageId,
                // SỬA: Gán DateOnly
                StartDate = today,
                // SỬA: Tinh toan EndDate (AddDays trả về DateTime, nên phải convert lại)
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(ParseDuration(packageToBuy.Duration))),
                Status = "Active"
            };

            _context.HostPackages.Add(newHostPackage);
            await _context.SaveChangesAsync();

            newHostPackage.Package = packageToBuy;
            return MapToDetailsDto(newHostPackage, 0);
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
                PackageName = hostPackage.Package.Name,
                Status = hostPackage.Status,
                StartDate = hostPackage.StartDate, // Model la DateOnly
                EndDate = hostPackage.EndDate,     // Model la DateOnly
                CurrentListings = currentListings,
                MaxListings = _featureService.GetMaxListingCount(hostPackage.PackageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(hostPackage.PackageId)
            };
        }
    }
}