using CondotelManagement.DTOs.Host;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace CondotelManagement.Controllers.Host
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostController : ControllerBase
    {
        private readonly IHostService _hostService;

        public HostController(IHostService hostService)
        {
            _hostService = hostService;
        }

        [HttpPost("register-as-host")]
        public async Task<IActionResult> RegisterHost([FromBody] HostRegisterRequestDto dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var userId = int.Parse(userIdString);

                // SỬA: Thay đổi biến nhận kết quả thành responseDto (loại bỏ lỗi Serialization)
                var responseDto = await _hostService.RegisterHostAsync(userId, dto);

                return Ok(new
                {
                    message = "Chúc mừng! Bạn đã đăng ký Host thành công.",
                    // Lấy ID từ DTO an toàn
                    hostId = responseDto.HostId
                });
            }
            catch (Exception ex)
            {
                // Lỗi thực tế (bao gồm lỗi SQL UNIQUE KEY cũ nếu chưa xóa) sẽ được trả về
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}