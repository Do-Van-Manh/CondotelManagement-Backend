    using CondotelManagement.Data;
    using CondotelManagement.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    namespace CondotelManagement.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class PackageController : ControllerBase
        {
            private readonly IPackageService _packageService;
            private readonly CondotelDbVer1Context _context;

            public PackageController(IPackageService packageService, CondotelDbVer1Context context)
            {
                _packageService = packageService;
                _context = context;
            }

            [HttpGet]
            public async Task<IActionResult> GetAvailablePackages()
            {
                var packages = await _packageService.GetAvailablePackagesAsync();
                return Ok(packages);
            }

            [HttpGet("confirm-payment")]
            public async Task<IActionResult> ConfirmPackagePayment(string orderCode)
            {
                try
                {
                    Console.WriteLine($"[CONFIRM] Start processing OrderCode: {orderCode}");

                    // 1. TÌM ĐƠN HÀNG ĐỂ LẤY THÔNG TIN CẦN THIẾT
                    // Chỉ Select những trường cần dùng để tối ưu
                    var packageOrder = await _context.HostPackages
                        .Include(hp => hp.Package)
                       .Where(hp => hp.OrderCode != null && hp.OrderCode == orderCode)
                        .Select(hp => new
                        {
                            hp.HostId,
                            hp.PackageId,
                            hp.Status,
                            hp.DurationDays,
                            PackageName = hp.Package.Name
                        })
                        .FirstOrDefaultAsync();

                    if (packageOrder == null)
                    {
                        return BadRequest("Không tìm thấy đơn hàng!");
                    }

                    // Nếu đã Active rồi thì báo thành công luôn
                    if (packageOrder.Status == "Active")
                    {
                        return Ok(new { message = "Đơn hàng đã kích hoạt trước đó!", roleUpgraded = true });
                    }

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var durationDays = packageOrder.DurationDays ?? 30; // Mặc định 30 nếu null
                    var endDate = today.AddDays(durationDays);

                    // ---------------------------------------------------------
                    // 2. CẬP NHẬT HOST PACKAGE (DÙNG EXECUTE UPDATE ASYNC) - QUAN TRỌNG
                    // ---------------------------------------------------------
                    // Thay vì gán property và SaveChanges, ta bắn lệnh Update trực tiếp
                    var rowsPackage = await _context.HostPackages
                        .Where(hp => hp.OrderCode != null && hp.OrderCode == orderCode)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(hp => hp.Status, "Active")
                            .SetProperty(hp => hp.StartDate, today)
                            .SetProperty(hp => hp.EndDate, endDate)
                        );

                    if (rowsPackage > 0)
                    {
                        Console.WriteLine($"✅ [SUCCESS] HostPackage đã Active. Start: {today}, End: {endDate}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ [FAIL] Không update được dòng HostPackage nào!");
                        // Nếu không update được package thì dừng luôn
                        return StatusCode(500, "Lỗi: Không thể kích hoạt gói dịch vụ trong Database.");
                    }

                    // ---------------------------------------------------------
                    // 3. CẬP NHẬT USER ROLE (GIỮ NGUYÊN VÌ ĐANG CHẠY TỐT)
                    // ---------------------------------------------------------
                    var hostInfo = await _context.Hosts
                        .Where(h => h.HostId == packageOrder.HostId)
                        .Select(h => new { h.UserId })
                        .FirstOrDefaultAsync();

                    if (hostInfo != null)
                    {
                        await _context.Users
                            .Where(u => u.UserId == hostInfo.UserId && u.RoleId != 4)
                            .ExecuteUpdateAsync(s => s.SetProperty(u => u.RoleId, 4));

                        Console.WriteLine($"✅ [SUCCESS] User {hostInfo.UserId} đã lên Role Host.");
                    }

                    // 4. TRẢ VỀ KẾT QUẢ
                    return Ok(new
                    {
                        message = "THANH TOÁN THÀNH CÔNG! BẠN ĐÃ CHÍNH THỨC TRỞ THÀNH HOST!",
                        roleUpgraded = true,
                        packageName = packageOrder.PackageName,
                        startDate = today.ToString("yyyy-MM-dd"),
                        endDate = endDate.ToString("yyyy-MM-dd"),
                        duration = $"{durationDays} ngày"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                    return StatusCode(500, "Lỗi server: " + ex.Message);
                }
            } }
        }