namespace CondotelManagement.Services.Interfaces.Shared
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendPasswordResetOtpAsync(string toEmail, string otp);
        Task SendVerificationOtpAsync(string toEmail, string otp);
    }
}
