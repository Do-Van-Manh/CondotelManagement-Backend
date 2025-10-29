using System.Security.Claims;
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Upload
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloud;
        private readonly IAuthRepository _repo;


        public UploadController(ICloudinaryService cloud, IAuthRepository repo)
        {
            _cloud = cloud;
            _repo = repo;
        }

        // Upload ảnh chung
        [HttpPost("image")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null) return BadRequest("No file uploaded");
            var url = await _cloud.UploadImageAsync(file);
            return Ok(new { imageUrl = url });
        }

        // ✅ Upload ảnh cho user hiện tại
        [HttpPost("user-image")]
        [Authorize] // ✅ Cho phép tất cả user đã đăng nhập
        public async Task<IActionResult> UploadUserImage(IFormFile file)
        {
            if (file == null)
                return BadRequest(new { message = "No file uploaded" });

            // Lấy email từ JWT token
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Invalid token" });

            // Upload ảnh lên Cloudinary
            var imageUrl = await _cloud.UploadImageAsync(file);

            // Cập nhật user trong DB
            var user = await _repo.GetByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.ImageUrl = imageUrl;
            await _repo.UpdateUserAsync(user);

            return Ok(new
            {
                message = "Profile image updated successfully",
                imageUrl
            });
        }


    }
}
