using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;

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

    public interface IEmailService
    {
        Task SendInactivityEmailAsync(string toEmail, string userName, DateTime lastLog);
        Task SendPasswordChangeEmailAsync(string toEmail, string userName, DateTime changeTime);

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
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.FromName, _mailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "We've Missed You - Your IBanking Account";

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            var logo = new LinkedResource(logoPath, "image/png") { ContentId = "logo" };

            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <img src='cid:logo' alt='IBanking Logo' style='max-height: 60px;'>
                    </div>
                    
                    <div style='padding: 30px;'>
                        <h2 style='color: #2c3e50;'>Hello {userName},</h2>
                        
                        <p style='color: #34495e; line-height: 1.6;'>
                            We noticed you haven't logged in to your IBanking account for
                            <strong> 30 days</strong>.
                        </p>
                        
                        <p style='color: #34495e; line-height: 1.6;'>
                            For security reasons, we recommend logging in to keep your account active and ensure uninterrupted access to our services.
                        </p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 0; color: #7f8c8d;'>
                                <strong>Last activity:</strong> {lastLog:dd MMMM yyyy 'at' HH:mm}
                            </p>
                        </div>
                        
                        <p style='color: #95a5a6; font-size: 14px;'>
                            If you didn't attempt to log in or believe this is a mistake, please contact our support team immediately.
                        </p>
                    </div>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; color: #7f8c8d; font-size: 12px;'>
                        <p>© {DateTime.Now.Year} IBanking. All rights reserved.</p>
                        <p>This is an automated message - please do not reply directly to this email.</p>
                    </div>
                </div>
            ";

            var image = bodyBuilder.LinkedResources.Add(logoPath);
            image.ContentId = "logo";

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(
                _mailSettings.Host,
                _mailSettings.Port,
                SecureSocketOptions.SslOnConnect
            );
            await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendPasswordChangeEmailAsync(string toEmail, string userName, DateTime changeTime)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.FromName, _mailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Password Updated - Your IBanKing Account";

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
            <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                <img src='cid:logo' alt='IBanking Logo' style='max-height: 60px;'>
            </div>
            
            <div style='padding: 30px;'>
                <h2 style='color: #2c3e50;'>Hello {userName},</h2>
                
                <p style='color: #34495e; line-height: 1.6;'>
                    This is a confirmation that your IBanKing password was successfully changed.
                </p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <p style='margin: 0; color: #7f8c8d;'>
                        <strong>Change time:</strong> {changeTime:dd MMMM yyyy 'at' HH:mm}
                    </p>
                </div>
                
                <p style='color: #34495e; line-height: 1.6;'>
                    If you didn't make this change, please contact our support team immediately as your account security might be compromised.
                </p>
               
            </div>
            
            <div style='background-color: #f8f9fa; padding: 20px; text-align: center; color: #7f8c8d; font-size: 12px;'>
                <p>© {DateTime.Now.Year} IBanKing. All rights reserved.</p>
                <p>This is an automated message - please do not reply directly to this email.</p>
            </div>
        </div>
    ";

            var image = bodyBuilder.LinkedResources.Add(logoPath);
            image.ContentId = "logo";

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
            await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    }
}