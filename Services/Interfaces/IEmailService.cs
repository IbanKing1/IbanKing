namespace IBanKing.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendInactivityEmailAsync(string toEmail, string userName, DateTime lastLog);
        Task SendPasswordChangeEmailAsync(string toEmail, string userName, DateTime changeTime);
        Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, decimal amount, string currency, string receiver, DateTime dateTime);
    }

}
