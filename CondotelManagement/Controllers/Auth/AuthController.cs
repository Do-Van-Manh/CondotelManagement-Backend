using CondotelManagement.DTOs.Auth;
using CondotelManagement.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password, or account not activated." }); // Sửa thông báo

            return Ok(result);
        }

        // THÊM MỚI: Endpoint Google Login
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var result = await _authService.GoogleLoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Google authentication failed." });

            return Ok(result);
        }

        // SỬA ĐỔI: Endpoint Register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var success = await _authService.RegisterAsync(request);
            if (!success)
                return BadRequest(new { message = "Email already exists and is activated." });

            // SỬA THÔNG BÁO: Yêu cầu xác thực OTP
            return StatusCode(201, new { message = "User registration initiated. Please check your email for an OTP to verify your account." });
        }

        // THÊM MỚI: Endpoint Xác thực Email
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var success = await _authService.VerifyEmailAsync(request);
            if (!success)
                return BadRequest(new { message = "Invalid email, incorrect OTP, or OTP has expired." });

            return Ok(new { message = "Email verified successfully. You can now log in." });
        }


        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful" });
        }

        // ... (Các endpoint forgot-password, send-otp, reset-password-with-otp, GetMe, admin-check giữ nguyên) ...

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }

        [HttpPost("send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordResetOtp([FromBody] ForgotPasswordRequest request)
        {
            var success = await _authService.SendPasswordResetOtpAsync(request);
            // (Giữ logic cũ)
            return Ok(new { message = "If your email is registered, you will receive an OTP code." });
        }

        [HttpPost("reset-password-with-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request)
        {
            var success = await _authService.ResetPasswordWithOtpAsync(request);
            if (!success)
                return BadRequest(new { message = "Failed to reset password. Invalid email, expired or incorrect OTP." });

            return Ok(new { message = "Password updated successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

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

        [HttpGet("admin-check")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminCheck()
        {
            return Ok(new { message = "Welcome, Admin!" });
        }
    }
}