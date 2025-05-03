using IBanKing.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace IBanKing.Services
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _env;

        public EmailService(IOptions<MailSettings> mailSettings, IWebHostEnvironment env)
        {
            _mailSettings = mailSettings.Value;
            _env = env;
        }

        public async Task SendInactivityEmailAsync(string toEmail, string userName, DateTime lastLog)
        {
            var email = CreateEmail(toEmail, "We've Missed You - Your IBanKing Account");
            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px;'>
                    <div style='background: #f8f9fa; padding: 20px; text-align: center;'>
                        <img src='cid:logo' style='max-height: 60px;' />
                    </div>
                    <div style='padding: 30px;'>
                        <h2>Hello {userName},</h2>
                        <p>You haven’t logged in to your IBanKing account for <strong>30 days</strong>.</p>
                        <p>For security, we recommend logging in to keep your account active.</p>
                        <div style='background: #eee; padding: 10px; margin: 20px 0;'>
                            <strong>Last activity:</strong> {lastLog:dd MMMM yyyy 'at' HH:mm}
                        </div>
                        <p style='font-size: 13px; color: #999;'>If this wasn't you, contact support immediately.</p>
                    </div>
                    <div style='text-align: center; background: #f8f9fa; padding: 20px; font-size: 12px; color: #999;'>
                        © {DateTime.Now.Year} IBanKing. This is an automated message.
                    </div>
                </div>"
            };

            bodyBuilder.LinkedResources.Add(logoPath).ContentId = "logo";
            email.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(email);
        }

        public async Task SendPasswordChangeEmailAsync(string toEmail, string userName, DateTime changeTime)
        {
            var email = CreateEmail(toEmail, "Password Updated - Your IBanKing Account");
            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px;'>
                    <div style='background: #f8f9fa; padding: 20px; text-align: center;'>
                        <img src='cid:logo' style='max-height: 60px;' />
                    </div>
                    <div style='padding: 30px;'>
                        <h2>Hello {userName},</h2>
                        <p>This is to confirm your password was successfully changed.</p>
                        <div style='background: #eee; padding: 10px; margin: 20px 0;'>
                            <strong>Change time:</strong> {changeTime:dd MMMM yyyy 'at' HH:mm}
                        </div>
                        <p style='font-size: 13px; color: #999;'>If this wasn't you, contact support immediately.</p>
                    </div>
                    <div style='text-align: center; background: #f8f9fa; padding: 20px; font-size: 12px; color: #999;'>
                        © {DateTime.Now.Year} IBanKing. This is an automated message.
                    </div>
                </div>"
            };

            bodyBuilder.LinkedResources.Add(logoPath).ContentId = "logo";
            email.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(email);
        }

        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, decimal amount, string currency, string receiver, DateTime dateTime)
        {
            var email = CreateEmail(toEmail, "Payment Confirmation - IBanKing");
            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px;'>
                    <div style='background: #f8f9fa; padding: 20px; text-align: center;'>
                        <img src='cid:logo' style='max-height: 60px;' />
                    </div>
                    <div style='padding: 30px;'>
                        <h2>Hello {userName},</h2>
                        <p>You’ve just completed a payment:</p>
                        <ul>
                            <li><strong>Receiver:</strong> {receiver}</li>
                            <li><strong>Amount:</strong> {amount:F2} {currency}</li>
                            <li><strong>Date:</strong> {dateTime:dd MMM yyyy HH:mm}</li>
                        </ul>
                        <p style='font-size: 13px; color: #999;'>If this wasn't you, please contact support.</p>
                    </div>
                    <div style='text-align: center; background: #f8f9fa; padding: 20px; font-size: 12px; color: #999;'>
                        © {DateTime.Now.Year} IBanKing. This is an automated message.
                    </div>
                </div>"
            };

            bodyBuilder.LinkedResources.Add(logoPath).ContentId = "logo";
            email.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(email);
        }

        private MimeMessage CreateEmail(string toEmail, string subject)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.FromName, _mailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            return email;
        }

        private async Task SendEmailAsync(MimeMessage email)
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
