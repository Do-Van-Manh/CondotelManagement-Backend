using CondotelManagement.Data;
using CondotelManagement.DTOs.Package;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CondotelManagement.Repositories.Interfaces.Admin;
using System.Text.RegularExpressions;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Payment;
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
        private readonly IPayOSService _payOSService;

        public PackageService(CondotelDbVer1Context context, IPackageFeatureService featureService, IUserRepository userRepo, IPayOSService payOSService)
        {
            _context = context;
            _featureService = featureService;
            _userRepo = userRepo;
            _payOSService = payOSService;
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Lấy package mới nhất của host (ưu tiên Active, sau đó PendingPayment)
            // Không filter EndDate để lấy cả package chưa active
            var hostPackage = await _context.HostPackages
                .Include(hp => hp.Package)
                .Where(hp => hp.HostId == hostId)
                .OrderByDescending(hp => hp.Status == "Active" ? 1 : 0) // Ưu tiên Active trước
                .ThenByDescending(hp => hp.StartDate ?? DateOnly.MinValue) // Sau đó sắp xếp theo StartDate
                .FirstOrDefaultAsync();

            if (hostPackage == null) return null;

            // Nếu package là Active nhưng đã hết hạn, vẫn trả về nhưng có thể thông báo
            if (hostPackage.Status == "Active" && hostPackage.EndDate.HasValue && hostPackage.EndDate < today)
            {
                // Package đã hết hạn nhưng vẫn trả về để hiển thị
            }

            var currentListings = await _context.Condotels
                .CountAsync(c => c.HostId == hostId && c.Status != "Deleted");

            return MapToDetailsDto(hostPackage, currentListings);
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
            // Lấy OrderCode và Amount từ package
            long orderCode = 0;
            if (!string.IsNullOrEmpty(hostPackage.OrderCode) && long.TryParse(hostPackage.OrderCode, out long parsedOrderCode))
            {
                orderCode = parsedOrderCode;
            }

            decimal amount = hostPackage.Package?.Price ?? 0;

            return new HostPackageDetailsDto
            {
                PackageName = hostPackage.Package?.Name ?? "Không xác định",
                Status = hostPackage.Status,
                StartDate = hostPackage.StartDate?.ToString("yyyy-MM-dd") ?? "",
                EndDate = hostPackage.EndDate?.ToString("yyyy-MM-dd") ?? "",
                CurrentListings = currentListings,
                MaxListings = _featureService.GetMaxListingCount(hostPackage.PackageId),
                CanUseFeaturedListing = _featureService.CanUseFeaturedListing(hostPackage.PackageId),
                Message = hostPackage.Status == "PendingPayment" ? "Đang chờ thanh toán" : null,
                PaymentUrl = null,
                OrderCode = orderCode,
                Amount = amount
            };
        }

        public async Task<CancelPackageResponseDTO> CancelPackageAsync(int hostId, CancelPackageRequestDTO request)
        {
            // Lấy package Active của host
            var hostPackage = await _context.HostPackages
                .Include(hp => hp.Package)
                .Include(hp => hp.Host)
                    .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(hp => hp.HostId == hostId && hp.Status == "Active");

            if (hostPackage == null)
            {
                return new CancelPackageResponseDTO
                {
                    Success = false,
                    Message = "Không tìm thấy package đang active để hủy."
                };
            }

            // Kiểm tra package đã được thanh toán chưa (có StartDate và EndDate)
            if (!hostPackage.StartDate.HasValue || !hostPackage.EndDate.HasValue)
            {
                // Package chưa được kích hoạt (chưa thanh toán) - chỉ cần hủy
                hostPackage.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return new CancelPackageResponseDTO
                {
                    Success = true,
                    Message = "Đã hủy package thành công (package chưa được thanh toán)."
                };
            }

            // Package đã được thanh toán - tính toán hoàn tiền
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = hostPackage.StartDate.Value;
            var endDate = hostPackage.EndDate.Value;
            var totalDays = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            var daysUsed = (today.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            var daysRemaining = totalDays - daysUsed;

            // Tính số tiền hoàn lại (theo tỷ lệ số ngày còn lại)
            var packagePrice = hostPackage.Package?.Price ?? 0;
            decimal refundAmount = 0;

            if (daysRemaining > 0 && totalDays > 0)
            {
                // Hoàn tiền theo tỷ lệ số ngày còn lại
                refundAmount = (packagePrice * daysRemaining) / totalDays;
                refundAmount = Math.Round(refundAmount, 2);
            }

            // Nếu số tiền hoàn lại >= 10,000 VND thì tạo refund payment link
            string? refundPaymentLink = null;
            if (refundAmount >= 10000)
            {
                try
                {
                    // Lấy thông tin host
                    var host = hostPackage.Host;
                    var user = host?.User;

                    // Tạo refund payment link qua PayOS
                    // Sử dụng OrderCode của package để tạo refund
                    if (!string.IsNullOrEmpty(hostPackage.OrderCode) && long.TryParse(hostPackage.OrderCode, out long orderCode))
                    {
                        // Tạo payment link để host nhận tiền hoàn lại
                        var refundResponse = await _payOSService.CreatePackageRefundPaymentLinkAsync(
                            hostId,
                            orderCode,
                            refundAmount,
                            user?.FullName ?? "Host",
                            user?.Email,
                            user?.Phone
                        );

                        if (refundResponse?.Data != null)
                        {
                            refundPaymentLink = refundResponse.Data.CheckoutUrl;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng vẫn tiếp tục hủy package
                    Console.WriteLine($"[CancelPackage] Error creating refund link: {ex.Message}");
                }
            }

            // Cập nhật status của HostPackage thành "Cancelled"
            hostPackage.Status = "Cancelled";
            await _context.SaveChangesAsync();

            // Hạ role của user về Tenant (RoleId = 3) nếu không còn package active nào
            var hasOtherActivePackage = await _context.HostPackages
                .AnyAsync(hp => hp.HostId == hostId && hp.Status == "Active" && hp.EndDate >= today);

            if (!hasOtherActivePackage && hostPackage.Host?.UserId != null)
            {
                var user = await _context.Users.FindAsync(hostPackage.Host.UserId);
                if (user != null && user.RoleId == 4) // RoleId 4 = Host
                {
                    user.RoleId = 3; // RoleId 3 = Tenant
                    await _context.SaveChangesAsync();
                }
            }

            return new CancelPackageResponseDTO
            {
                Success = true,
                Message = refundAmount >= 10000 
                    ? "Đã hủy package thành công. Link hoàn tiền đã được tạo." 
                    : "Đã hủy package thành công. Số tiền hoàn lại không đủ tối thiểu (10,000 VND).",
                RefundAmount = refundAmount >= 10000 ? refundAmount : null,
                RefundPaymentLink = refundPaymentLink,
                DaysUsed = daysUsed,
                DaysRemaining = daysRemaining > 0 ? daysRemaining : 0
            };
        }
    }
}