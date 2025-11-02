using BCrypt.Net;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Admin; // Giả sử IUserRepository ở đây
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.Shared;
using Google.Apis.Auth;
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
        private readonly IUserRepository _userRepo;

        public AuthService(IAuthRepository repo, IConfiguration config, IHttpContextAccessor httpContextAccessor, IEmailService emailService, IUserRepository userRepo)
        {
            _repo = repo;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _userRepo = userRepo;
        }

        // SỬA LỖI 1 (Phần 1): Gọi hàm helper
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null || user.Status != "Active")
                return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // Gọi hàm helper
            return GenerateJwtToken(user);
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // 1. Kiểm tra xem email đã tồn tại và Active chưa
            var existingUser = await _repo.GetByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Status == "Active")
            {
                return false; // Email đã được đăng ký và kích hoạt
            }

            // 2. Tạo OTP
            string otp = new Random().Next(100000, 999999).ToString();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10); // OTP hết hạn sau 10 phút

            User userToRegister;

            if (existingUser != null && existingUser.Status == "Pending")
            {
                // 3a. Nếu user tồn tại nhưng "Pending" (chưa kích hoạt)
                // Cập nhật lại mật khẩu và thông tin, gửi lại OTP
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                existingUser.FullName = request.FullName;
                existingUser.Phone = request.Phone;
                existingUser.Gender = request.Gender;
                existingUser.DateOfBirth = request.DateOfBirth;
                existingUser.Address = request.Address;

                await _userRepo.UpdateUserAsync(existingUser); // Cần IUserRepository
                userToRegister = existingUser;
            }
            else
            {
                // 3b. Tạo user mới hoàn toàn với Status = "Pending"
                userToRegister = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Phone = request.Phone,
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    RoleId = 3, // Role "User"
                    Status = "Pending", // QUAN TRỌNG
                    CreatedAt = DateTime.UtcNow
                };
                await _repo.RegisterAsync(userToRegister);
            }

            // 4. Lưu OTP (dùng chung cột ResetToken) và gửi mail
            // Giả định SetPasswordResetTokenAsync lưu vào 2 cột PasswordResetToken và ResetTokenExpires
            await _repo.SetPasswordResetTokenAsync(userToRegister, otp, expiry);
            await _emailService.SendVerificationOtpAsync(userToRegister.Email, otp);

            return true;
        }

        // SỬA LỖI 2 & 3: Đổi ResetToken -> PasswordResetToken
        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);

            // Kiểm tra user, status, OTP và thời gian hết hạn
            if (user == null ||
                user.Status != "Pending" || // Chỉ xác thực user "Pending"
                user.PasswordResetToken != request.Otp || // <-- SỬA Ở ĐÂY
                user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false; // Sai OTP, hết hạn, hoặc email không hợp lệ
            }

            // 1. Kích hoạt tài khoản
            user.Status = "Active";
            // 2. Xóa OTP
            user.PasswordResetToken = null; // <-- SỬA Ở ĐÂY
            user.ResetTokenExpires = null;

            // 3. Cập nhật user (Cần IUserRepository)
            return await _userRepo.UpdateUserAsync(user);
        }

        // THÊM MỚI: Logic Google Login
        public async Task<LoginResponse?> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                // 1. Xác thực IdToken từ Google
                var googleClientId = _config["Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                // 2. Lấy thông tin
                var userEmail = payload.Email;
                var userName = payload.Name;

                // 3. Kiểm tra user trong DB
                var user = await _repo.GetByEmailAsync(userEmail);

                if (user == null)
                {
                    // 3a. Nếu user không tồn tại -> Tự động đăng ký
                    user = new User
                    {
                        FullName = userName,
                        Email = userEmail,
                        PasswordHash = "EXTERNAL_LOGIN", // Không dùng mật khẩu
                        RoleId = 3, // Role "User"
                        Status = "Active", // Google đã xác thực email
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.RegisterAsync(user);

                    // Lấy lại user (đã có UserId)
                    user = await _repo.GetByEmailAsync(userEmail);
                }

                // 4. Đảm bảo user "Active" (phòng trường hợp user đã tồn tại nhưng "Pending")
                if (user.Status != "Active")
                {
                    user.Status = "Active";
                    await _userRepo.UpdateUserAsync(user); // Cần IUserRepository
                }

                // 5. Tạo và trả về JWT token của hệ thống
                return GenerateJwtToken(user); // Lỗi CS0103 xảy ra ở đây
            }
            catch (Exception ex)
            {
                // Log lỗi (ex.Message)
                return null; // Token Google không hợp lệ
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null || user.Status != "Active")
            {
                return true;
            }
            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.UtcNow.AddHours(1);
            await _repo.SetPasswordResetTokenAsync(user, token, expiry);
            string resetLink = $"https://your-frontend-domain.com/reset-password?token={token}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            return true;
        }


        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _repo.GetUserByResetTokenAsync(request.ResetToken);
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false;
            }
            string newHashStr = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var success = await _repo.UpdatePasswordAsync(user.Email, newHashStr);
            if (!success) return false;
            await _repo.SetPasswordResetTokenAsync(user, null, DateTime.UtcNow);
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

        public async Task<bool> SendPasswordResetOtpAsync(ForgotPasswordRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null || user.Status != "Active")
            {
                return true;
            }
            string otp = new Random().Next(100000, 999999).ToString();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);
            await _repo.SetPasswordResetTokenAsync(user, otp, expiry);
            await _emailService.SendPasswordResetOtpAsync(user.Email, otp);
            return true;
        }

        public async Task<bool> ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);

            // Tên cột ở đây (PasswordResetToken) đã đúng
            if (user == null ||
                user.PasswordResetToken != request.Otp ||
                user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false;
            }

            string newHashStr = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var success = await _repo.UpdatePasswordAsync(user.Email, newHashStr);
            if (!success) return false;
            await _repo.SetPasswordResetTokenAsync(user, null, DateTime.UtcNow);
            return true;
        }

        // SỬA LỖI 1 (Phần 2): Thêm hàm helper bị thiếu
        private LoginResponse GenerateJwtToken(User user)
        {
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
    }
}