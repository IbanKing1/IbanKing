using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IBanKing.Pages.MakePayment
{
    public class Step3Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public Step3Model(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public Transaction ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData["ReceiverIBAN"] == null || TempData["Amount"] == null)
                return RedirectToPage("Index");

            ViewModel.ReceiverIBAN = TempData["ReceiverIBAN"].ToString();
            ViewModel.Amount = double.Parse(TempData["Amount"].ToString(), CultureInfo.InvariantCulture);
            ViewModel.Currency = TempData["Currency"]?.ToString() ?? "RON";
            TempData.Keep();

            decimal amountDecimal = Convert.ToDecimal(ViewModel.Amount, CultureInfo.InvariantCulture);

            var userIdStr = HttpContext.Session.GetString("UserId") ?? TempData["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ViewModel.Status = "error";
                ViewModel.Message = "User is not authenticated.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "User not found.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(ViewModel.ReceiverIBAN) || ViewModel.ReceiverIBAN.Length < 10)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Invalid IBAN.";
                return Page();
            }

            var account = _context.Accounts.FirstOrDefault(a => a.UserId == userId && a.Currency == ViewModel.Currency);
            if (account == null)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "No account found in selected currency.";
                return Page();
            }

            if (account.Balance < amountDecimal)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Insufficient funds.";
                return Page();
            }

            decimal convertedAmount = amountDecimal;
            int? exchangeRateId = null;

            var exchange = _context.ExchangeRates.FirstOrDefault(e => e.FromCurrency == ViewModel.Currency && e.ToCurrency == "RON");
            if (exchange != null)
            {
                convertedAmount = amountDecimal * (decimal)exchange.Rate;
                exchangeRateId = exchange.ExchangeRateId;
            }

            var servicedPayment = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == ViewModel.ReceiverIBAN);
            int? servicedPaymentId = servicedPayment?.ServicedPaymentId;
            ViewModel.ServicedPaymentName = servicedPayment?.Bill_Name;

            account.Balance -= amountDecimal;
            _context.Accounts.Update(account);

            var transaction = new Transaction
            {
                Sender = $"USER-{userId}",
                Receiver = ViewModel.ReceiverIBAN,
                Amount = (double)convertedAmount,
                Currency = ViewModel.Currency,
                DateTime = DateTime.UtcNow,
                UserId = userId,
                ExchangeRateId = exchangeRateId,
                ServicedPaymentId = servicedPaymentId,
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("noreply@ibanking.com"),
                    Subject = "Payment Confirmation",
                    Body = $"Your payment of {ViewModel.Amount} {ViewModel.Currency} to {ViewModel.ReceiverIBAN} was successful.",
                    IsBodyHtml = false
                };
                mail.To.Add(user.Email);

                using (var smtp = new SmtpClient("localhost"))
                {
                    smtp.Send(mail);
                }
            }
            catch { }

            var receiverAccount = _context.Accounts.FirstOrDefault(a => a.IBAN == ViewModel.ReceiverIBAN);
            if (receiverAccount != null)
            {
                await _notificationService.CreatePaymentNotification(
                    receiverAccount.UserId.ToString(),
                    transaction.TransactionId,
                    amountDecimal);
            }

            ViewModel.Status = "success";
            ViewModel.Message = "Payment completed and saved successfully.";
            return Page();
        }
    }
}