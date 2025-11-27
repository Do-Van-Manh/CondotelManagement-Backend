// File: Controllers/Admin/AdminPackageController.cs
using CondotelManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/packages")]
    [Authorize(Roles = "Admin")]  // chỉ Admin vào được
    public class AdminPackageController : ControllerBase
    {
        private readonly CondotelDbVer1Context _context;

        public AdminPackageController(CondotelDbVer1Context context)
        {
            _context = context;
        }

        // GET: api/admin/packages?search=abc
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var query = _context.HostPackages
                .Include(hp => hp.Host!)
                    .ThenInclude(h => h.User!)
                .Include(hp => hp.Package!)
                .AsQueryable();

            // Sắp xếp trước khi Where để tránh lỗi OrderedQueryable
            query = query.OrderByDescending(hp => hp.HostPackageId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(hp =>
                    hp.OrderCode.Contains(search) ||
                    (hp.Host != null && hp.Host.User != null && (
                        (hp.Host.User.Email != null && hp.Host.User.Email.ToLower().Contains(search)) ||
                        (hp.Host.User.Phone != null && hp.Host.User.Phone.Contains(search)) ||
                        (hp.Host.User.FullName != null && hp.Host.User.FullName.ToLower().Contains(search))
                    ))
                );
            }

            var result = await query.Select(hp => new
            {
                hp.HostPackageId,
                HostName = hp.Host != null && hp.Host.User != null ? (hp.Host.User.FullName ?? "Chưa có tên") : "Không xác định",
                Email = hp.Host != null && hp.Host.User != null ? (hp.Host.User.Email ?? "-") : "-",
                Phone = hp.Host != null && hp.Host.User != null ? (hp.Host.User.Phone ?? "-") : "-",
                PackageName = hp.Package != null ? hp.Package.Name : "Không xác định",
                hp.OrderCode,
                Amount = hp.Package != null ? (hp.Package.Price ?? 0) : 0,
                hp.Status,
                StartDate = hp.StartDate.HasValue ? hp.StartDate.Value.ToString("dd/MM/yyyy") : "-",
                EndDate = hp.EndDate.HasValue ? hp.EndDate.Value.ToString("dd/MM/yyyy") : "-",
                CanActivate = hp.Status == "PendingPayment"
            }).ToListAsync();

            return Ok(result);
        }

        // POST: api/admin/packages/123/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ManualActivate(int id)
        {
            var hp = await _context.HostPackages
                .Include(h => h.Host).ThenInclude(h => h!.User)
                .FirstOrDefaultAsync(h => h.HostPackageId == id);

            if (hp == null) return NotFound("Không tìm thấy đơn hàng");

            if (hp.Status == "Active")
                return BadRequest("Gói này đã được kích hoạt rồi!");

            var today = DateOnly.FromDateTime(DateTime.Today);
            var duration = hp.DurationDays ?? 30;

            hp.Status = "Active";
            hp.StartDate = today;
            hp.EndDate = today.AddDays(duration);

            // Nâng role Host lên 4 (nếu chưa phải)
            if (hp.Host?.User != null && hp.Host.User.RoleId != 4)
            {
                hp.Host.User.RoleId = 4;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã kích hoạt gói thành công cho Host!", hostName = hp.Host?.User?.FullName });
        }
    }
}