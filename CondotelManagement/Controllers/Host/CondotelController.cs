using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/[controller]")]
    [Authorize(Roles = "Host")]
    public class CondotelController : ControllerBase
    {
        private readonly ICondotelService _condotelService;
        private readonly IHostService _hostService;
        private readonly IPackageFeatureService _featureService; // THÊM DÒNG NÀY
        private readonly CondotelDbVer1Context _context;

        public CondotelController(ICondotelService condotelService, IHostService hostService, IPackageFeatureService featureService,CondotelDbVer1Context context)
        {
            _condotelService = condotelService;
            _hostService = hostService;
            _featureService = featureService;
            _context = context;
        }

        //GET /api/condotel
        [HttpGet]
        public ActionResult<IEnumerable<CondotelDTO>> GetAllCondotelByHost()
        {
            //current host login
            var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
            var condotels = _condotelService.GetCondtelsByHost(hostId);
            return Ok(condotels);
        }

        //GET /api/condotel/{id}
        [HttpGet("{id}")]
        public ActionResult<CondotelDetailDTO> GetById(int id)
        {
            var condotel = _condotelService.GetCondotelById(id);
            if (condotel == null)
                return NotFound(new { message = "Condotel not found" });

            return Ok(condotel);
        }

        //POST /api/condotel
        [HttpPost]
        public ActionResult Create([FromBody] CondotelCreateDTO condotelDto)
        {
            if (condotelDto == null)
                return BadRequest(new { message = "Invalid condotel data" });

            try
            {
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found or unauthorized" });

                // LẤY GÓI HIỆN TẠI CỦA HOST
                var activePackage = _context.HostPackages
                    .Include(hp => hp.Package)
                    .Where(hp => hp.HostId == host.HostId && hp.Status == "Active")
                    .OrderByDescending(hp => hp.EndDate)
                    .FirstOrDefault();

                int maxListings = 0;
                if (activePackage != null)
                {
                    maxListings = _featureService.GetMaxListingCount(activePackage.PackageId);
                }

                // ĐẾM SỐ CONDOTEL HIỆN TẠI CỦA HOST
                var currentCount = _context.Condotels
                    .Count(c => c.HostId == host.HostId && c.Status != "Deleted");

                // KIỂM TRA GIỚI HẠN
                if (currentCount >= maxListings)
                {
                    return StatusCode(403, new
                    {
                        message = $"Bạn đã đạt giới hạn đăng tin. Gói hiện tại chỉ cho phép tối đa {maxListings} condotel.",
                        currentCount,
                        maxListings,
                        upgradeRequired = maxListings == 0 || maxListings < 10 // gợi ý nâng cấp
                    });
                }

                condotelDto.HostId = host.HostId;
                var created = _condotelService.CreateCondotel(condotelDto);

                return CreatedAtAction(nameof(GetById),
                    new { id = created.CondotelId },
                    new { message = "Condotel created successfully", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        //PUT /api/condotel/{id}
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] CondotelUpdateDTO condotelDto)
        {
            if (condotelDto == null)
                return BadRequest(new { message = "Invalid condotel data" });

            if (condotelDto.CondotelId != id)
                return BadRequest(new { message = "Condotel ID mismatch" });

            try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found or unauthorized" });

                // Kiểm tra ownership - đảm bảo condotel thuộc về host này
                var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
                    return NotFound(new { message = "Condotel not found" });

                if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, new { message = "You do not have permission to update this condotel" });

                // Set HostId từ authenticated user (không cho client set)
                condotelDto.HostId = host.HostId;

                var updated = _condotelService.UpdateCondotel(condotelDto);
                if (updated == null)
                    return NotFound(new { message = "Condotel not found" });

                return Ok(new { message = "Condotel updated successfully", data = updated });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating condotel", error = ex.Message });
            }
        }

        //DELETE /api/condotel/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found or unauthorized" });

                // Kiểm tra ownership - đảm bảo condotel thuộc về host này
                var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
                    return NotFound(new { message = "Condotel not found" });

                if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, new { message = "You do not have permission to delete this condotel" });

                var success = _condotelService.DeleteCondotel(id);
                if (!success)
                    return NotFound(new { message = "Condotel not found or already deleted" });

                return Ok(new { message = "Condotel deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting condotel", error = ex.Message });
            }
        }
    }
}
