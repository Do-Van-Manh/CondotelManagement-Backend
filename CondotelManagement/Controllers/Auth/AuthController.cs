using CondotelManagement.DTOs.Auth; // Cần DTOs
using CondotelManagement.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous] // Cho phép truy cập không cần token
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" }); // Trả về 401

            return Ok(result); // Trả về 200
        }

        // THÊM MỚI
        [HttpPost("register")]
        [AllowAnonymous] // Cho phép truy cập không cần token
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var success = await _authService.RegisterAsync(request);
            if (!success)
                return BadRequest(new { message = "Email already exists or invalid data." }); // Trả về 400

            // Trả về 201 Created là chuẩn RESTful khi tạo mới
            return StatusCode(201, new { message = "User registered successfully." });
        }

        [HttpPost("logout")]
        [Authorize] // SỬA ĐỔI: Yêu cầu phải đăng nhập mới được logout
        public IActionResult Logout()
        {
            // Trong thực tế, bạn có thể cần vô hiệu hóa token (nếu dùng blacklist)
            // ở đây chúng ta chỉ trả về 200 OK
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);

            // SỬA ĐỔI: Luôn trả về 200 để bảo mật
            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }

        [HttpPost("send-otp")] // Đổi tên từ "forgot-password"
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordResetOtp([FromBody] ForgotPasswordRequest request) // DTO này chỉ cần Email
        {
            var success = await _authService.SendPasswordResetOtpAsync(request);
            if (!success)
            {
                // Vẫn trả về 200 để tránh lộ thông tin email nào có/không có trong hệ thống
                return Ok(new { message = "If your email is registered, you will receive an OTP code." });
            }
            return Ok(new { message = "If your email is registered, you will receive an OTP code." });
        }

        /// <summary>
        /// 2. Đặt lại mật khẩu bằng Email, OTP và Mật khẩu mới
        /// </summary>
        [HttpPost("reset-password-with-otp")] // Đổi tên từ "reset-password"
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request) // Dùng DTO mới
        {
            var success = await _authService.ResetPasswordWithOtpAsync(request);
            if (!success)
                return BadRequest(new { message = "Failed to reset password. Invalid email, expired or incorrect OTP." }); // 400

            return Ok(new { message = "Password updated successfully" }); // 200
        }

        // THÊM MỚI: Endpoint để lấy thông tin user hiện tại
        [HttpGet("me")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetMe()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            // Map User model sang UserProfileDto để trả về an toàn
            var userProfile = new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleName = user.Role?.RoleName ?? "User",
                Status = user.Status,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfile);
        }

        // THÊM MỚI: Endpoint ví dụ cho phân quyền
        [HttpGet("admin-check")]
        [Authorize(Roles = "Admin")] // Yêu cầu xác thực VÀ có Role "Admin"
        public IActionResult AdminCheck()
        {
            return Ok(new { message = "Welcome, Admin!" });
        }
    }
}