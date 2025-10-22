using CondotelManagement.Services.Interfaces.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CondotelManagement.Services.Implementations.Shared
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Reset Your Password - Condotel Management";

            var body = new BodyBuilder
            {
                HtmlBody = $"Please reset your password by <a href='{resetLink}'>clicking here</a>."
            };
            email.Body = body.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
