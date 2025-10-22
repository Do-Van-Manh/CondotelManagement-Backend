namespace CondotelManagement.Services.Interfaces.Shared
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
