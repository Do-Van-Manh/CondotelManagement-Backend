namespace CondotelManagement.Services.Interfaces.Shared
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendPasswordResetOtpAsync(string toEmail, string otp);
        Task SendVerificationOtpAsync(string toEmail, string otp);
        Task SendRefundConfirmationEmailAsync(string toEmail, string customerName, int bookingId, decimal refundAmount, string? bankCode = null, string? accountNumber = null);
        Task SendPayoutConfirmationEmailAsync(string toEmail, string hostName, int bookingId, string condotelName, decimal amount, DateTime paidAt, string? bankName = null, string? accountNumber = null, string? accountHolderName = null);
        Task SendPayoutAccountErrorEmailAsync(string toEmail, string hostName, int bookingId, string condotelName, decimal amount, string? currentBankName = null, string? currentAccountNumber = null, string? currentAccountHolderName = null, string? errorMessage = null);
    }
}
