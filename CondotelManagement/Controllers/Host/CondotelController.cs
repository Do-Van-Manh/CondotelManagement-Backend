using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var host = _hostService.GetByUserId(User.GetUserId());
            if (host == null)
                return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy host. Vui lòng đăng ký làm host trước."));
            
            var hostId = host.HostId;
            var condotels = _condotelService.GetCondtelsByHost(hostId);
            return Ok(ApiResponse<object>.SuccessResponse(condotels));
        }

        //GET /api/condotel/{id}
        [HttpGet("{id}")]
        public ActionResult<CondotelDetailDTO> GetById(int id)
        {
            var condotel = _condotelService.GetCondotelById(id);
            if (condotel == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy căn hộ khách sạn"));

            return Ok(ApiResponse<object>.SuccessResponse(condotel));
        }

        //POST /api/condotel
        [HttpPost]
        public ActionResult Create([FromBody] CondotelCreateDTO condotelDto)
        {
            if (condotelDto == null)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu condotel không hợp lệ"));

			if (condotelDto.Prices != null && condotelDto.Prices.Count > 0)
			{
				for (int i = 0; i < condotelDto.Prices.Count; i++)
				{
					var price = condotelDto.Prices[i];

					// Check Start < End
					if (price.StartDate >= price.EndDate)
					{
						ModelState.AddModelError($"Prices[{i}].StartDate", "StartDate phải nhỏ hơn EndDate.");
						ModelState.AddModelError($"Prices[{i}].EndDate", "EndDate phải lớn hơn StartDate.");
					}
				}
			}

			// Validate DataAnnotation
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail(ModelState.ToErrorDictionary()));
			}

			try
            {
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy host. Vui lòng đăng ký làm host trước."));

                // LẤY GÓI HIỆN TẠI CỦA HOST
                var activePackage = _context.HostPackages
                    .Include(hp => hp.Package)
                    .Where(hp => hp.HostId == host.HostId && hp.Status == "Active")
                    .OrderByDescending(hp => hp.EndDate)
                    .FirstOrDefault();

                var maxListings = activePackage != null
    ? _featureService.GetMaxListingCount(activePackage.PackageId)
    : 0;

                // ĐẾM SỐ CONDOTEL HIỆN TẠI CỦA HOST
                var currentCount = _context.Condotels
                    .Count(c => c.HostId == host.HostId && c.Status != "Deleted");
                if (currentCount >= maxListings)
                {
                    string message = maxListings == 0
                        ? "Bạn chưa có gói dịch vụ nào. Vui lòng mua gói để đăng tin."
                        : $"Bạn đã đạt giới hạn đăng tin ({currentCount}/{maxListings}). Vui lòng nâng cấp gói để đăng thêm.";

                    return StatusCode(403, new
                    {
                        success = false,
                        message,
                        currentCount,
                        maxListings,
                        upgradeRequired = true
                    });
                }
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

				return Ok(ApiResponse<object>.SuccessResponse(created, "Tạo condotel thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        //PUT /api/condotel/{id}
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] CondotelUpdateDTO condotelDto)
        {
            if (condotelDto == null)
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu condotel không hợp lệ"));

			if (condotelDto.CondotelId != id)
                return BadRequest(ApiResponse<object>.Fail("ID Condotel không khớp"));

			if (condotelDto.Prices != null && condotelDto.Prices.Count > 0)
			{
				for (int i = 0; i < condotelDto.Prices.Count; i++)
				{
					var price = condotelDto.Prices[i];

					// Check Start < End
					if (price.StartDate >= price.EndDate)
					{
						ModelState.AddModelError($"Prices[{i}].StartDate", "StartDate phải nhỏ hơn EndDate.");
						ModelState.AddModelError($"Prices[{i}].EndDate", "EndDate phải lớn hơn StartDate.");
					}
				}
			}

			// Validate DataAnnotation
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail(ModelState.ToErrorDictionary()));
			}

			try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
					return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy host. Vui lòng đăng ký làm host trước."));

				// Kiểm tra ownership - đảm bảo condotel thuộc về host này
				var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy condotel"));

                if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, ApiResponse<object>.Fail("Bạn không có quyền cập nhật căn hộ này"));

                // Set HostId từ authenticated user (không cho client set)
                condotelDto.HostId = host.HostId;

                var updated = _condotelService.UpdateCondotel(condotelDto);
                if (updated == null)
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy condotel"));
				return Ok(ApiResponse<object>.SuccessResponse(updated, "Condotel đã được cập nhật thành công"));
            }
            catch (ArgumentNullException ex)
            {
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
            catch (ArgumentException ex)
            {
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống: " + ex.Message));
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
					return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy host. Vui lòng đăng ký làm host trước."));

				// Kiểm tra ownership - đảm bảo condotel thuộc về host này
				var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy condotel"));

				if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, ApiResponse<object>.Fail("Bạn không có quyền xóa căn hộ này"));

                var success = _condotelService.DeleteCondotel(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Condotel không tìm thấy hoặc đã bị xóa"));

                return Ok(ApiResponse<object>.SuccessResponse("Condotel đã xóa thành công"));
            }
            catch (Exception ex)
            {
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống: " + ex.Message));
			}
        }
    }
}
