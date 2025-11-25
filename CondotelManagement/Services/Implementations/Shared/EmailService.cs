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
        public async Task SendPasswordResetOtpAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Your Password Reset OTP - Condotel Management";

            // Sửa lại nội dung email để hiển thị OTP
            var body = new BodyBuilder
            {
                HtmlBody = $"<p>Your password reset OTP code is:</p>" +
                           $"<h1 style='font-size: 24px; font-weight: bold; color: #333;'>{otp}</h1>" +
                           $"<p>This code will expire in 10 minutes.</p>" +
                           $"<p>If you did not request this, please ignore this email.</p>"
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

        public async Task SendVerificationOtpAsync(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Verify Your Email - Condotel Management";

            var body = new BodyBuilder
            {
                HtmlBody = $"<p>Thank you for registering. Your email verification OTP code is:</p>" +
                           $"<h1 style='font-size: 24px; font-weight: bold; color: #333;'>{otp}</h1>" +
                           $"<p>This code will expire in 10 minutes.</p>"
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

        public async Task SendRefundConfirmationEmailAsync(string toEmail, string customerName, int bookingId, decimal refundAmount, string? bankCode = null, string? accountNumber = null)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = $"Xác nhận hoàn tiền thành công - Booking #{bookingId}";

            // Format số tiền
            var formattedAmount = refundAmount.ToString("N0").Replace(",", ".") + " VNĐ";
            
            // Tạo nội dung email đẹp
            var bankInfoHtml = "";
            if (!string.IsNullOrEmpty(bankCode) && !string.IsNullOrEmpty(accountNumber))
            {
                bankInfoHtml = $@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Ngân hàng:</strong></td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>{bankCode}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Số tài khoản:</strong></td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>{accountNumber}</td>
                    </tr>";
            }

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .success-icon {{ font-size: 48px; margin-bottom: 20px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .amount {{ font-size: 24px; font-weight: bold; color: #28a745; margin: 10px 0; }}
        table {{ width: 100%; border-collapse: collapse; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>✓</div>
            <h1>Hoàn tiền thành công!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            
            <p>Chúng tôi xin thông báo rằng yêu cầu hoàn tiền cho booking <strong>#{bookingId}</strong> đã được xử lý thành công.</p>
            
            <div class='info-box'>
                <h3 style='margin-top: 0; color: #667eea;'>Thông tin hoàn tiền</h3>
                <table>
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Mã booking:</strong></td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>#{bookingId}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Số tiền hoàn lại:</strong></td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'><span class='amount'>{formattedAmount}</span></td>
                    </tr>
                    {bankInfoHtml}
                    <tr>
                        <td style='padding: 10px;'><strong>Trạng thái:</strong></td>
                        <td style='padding: 10px;'><span style='color: #28a745; font-weight: bold;'>Đã hoàn tiền</span></td>
                    </tr>
                </table>
            </div>
            
            <p>Tiền hoàn lại sẽ được chuyển vào tài khoản của bạn trong vòng 1-3 ngày làm việc (tùy thuộc vào ngân hàng).</p>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline.</p>
            
            <p>Trân trọng,<br>
            <strong>Đội ngũ Condotel Management</strong></p>
            
            <div class='footer'>
                <p>Email này được gửi tự động, vui lòng không trả lời email này.</p>
            </div>
        </div>
    </div>
</body>
</html>";

            var body = new BodyBuilder
            {
                HtmlBody = htmlBody
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

