namespace CondotelManagement.DTOs.Auth
{
    public class ResetPasswordWithOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; } // Nhận OTP từ người dùng
        public string NewPassword { get; set; }
    }
}
