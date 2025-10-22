using CondotelManagement.DTOs.Auth;
using CondotelManagement.Models; // Cần User model

namespace CondotelManagement.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request); // THÊM MỚI
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<User?> GetCurrentUserAsync(); // THÊM MỚI
    }
}