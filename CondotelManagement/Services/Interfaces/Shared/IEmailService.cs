namespace CondotelManagement.Services.Interfaces.Shared
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendPasswordResetOtpAsync(string toEmail, string otp);
        Task SendVerificationOtpAsync(string toEmail, string otp);
        Task SendRefundConfirmationEmailAsync(string toEmail, string customerName, int bookingId, decimal refundAmount, string? bankCode = null, string? accountNumber = null);
    }
}
