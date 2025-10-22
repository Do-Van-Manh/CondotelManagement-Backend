using BCrypt.Net;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Shared;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CondotelManagement.Services.Implementations.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository repo, IConfiguration config, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _repo = repo;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null || user.Status != "Active")
                return null;

            // SỬA ĐỔI: Không cần GetString, so sánh string trực tiếp
            // var storedHash = Encoding.UTF8.GetString(user.PasswordHash); // <-- BỎ DÒNG NÀY

            // PasswordHash bây giờ đã là string
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var roleName = user.Role?.RoleName ?? "User";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                RoleName = roleName,
                FullName = user.FullName
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            if (await _repo.CheckEmailExistsAsync(request.Email))
            {
                return false;
            }

            // 2. Hash mật khẩu (kết quả là string)
            string newHashStr = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // SỬA ĐỔI: Không cần chuyển sang byte[]
            // byte[] newHashBytes = Encoding.UTF8.GetBytes(newHashStr); // <-- BỎ DÒNG NÀY

            // 3. Tạo đối tượng User mới
            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = newHashStr, // <-- SỬA ĐỔI: Lưu trực tiếp string
                Phone = request.Phone,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                RoleId = 3,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            // 4. Lưu vào DB
            await _repo.RegisterAsync(newUser);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);

            // Luôn trả về true để bảo mật
            if (user == null || user.Status != "Active")
            {
                return true;
            }

            // 1. Tạo Token
            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ

            // 2. Lưu Token vào DB
            await _repo.SetPasswordResetTokenAsync(user, token, expiry);

            // 3. Gửi Mail
            // QUAN TRỌNG: Sửa link này thành link trang frontend của bạn
            string resetLink = $"https://your-frontend-domain.com/reset-password?token={token}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return true;
        }

        // SỬA LẠI ResetPasswordAsync
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // 1. Tìm user bằng Token
            var user = await _repo.GetUserByResetTokenAsync(request.ResetToken);

            // 2. Kiểm tra user và thời gian hết hạn
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false; // Token không hợp lệ hoặc đã hết hạn
            }

            // 3. Hash mật khẩu mới
            string newHashStr = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 4. Cập nhật mật khẩu
            var success = await _repo.UpdatePasswordAsync(user.Email, newHashStr);
            if (!success) return false;

            // 5. Vô hiệu hóa Token (quan trọng)
            await _repo.SetPasswordResetTokenAsync(user, null, DateTime.UtcNow); // Ghi đè token = null

            return true;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return await _repo.GetByEmailAsync(email);
        }
    }
}