using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace IBanKing.Pages.MakePayment
{
    public class Step3Model : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Step3Model(ApplicationDbContext context)
        {
            _context = context;
        }

        public Transaction ViewModel { get; set; } = new();

        public IActionResult OnGet()
        {
            // 1. Citim din TempData
            if (TempData["ReceiverIBAN"] == null || TempData["Amount"] == null)
                return RedirectToPage("Index");

            ViewModel.ReceiverIBAN = TempData["ReceiverIBAN"].ToString();
            ViewModel.Amount = double.Parse(TempData["Amount"].ToString(), CultureInfo.InvariantCulture);
            ViewModel.Currency = TempData["Currency"]?.ToString() ?? "RON";
            TempData.Keep();

            // Convertim Amount la decimal pentru operații ulterioare
            decimal amountDecimal = Convert.ToDecimal(ViewModel.Amount, CultureInfo.InvariantCulture);

            // 2. Verificare sesiune și user
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

            // 3. Validare IBAN
            if (string.IsNullOrWhiteSpace(ViewModel.ReceiverIBAN) || ViewModel.ReceiverIBAN.Length < 10)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Invalid IBAN.";
                return Page();
            }

            // 4. Căutăm contul utilizatorului în valuta selectată
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
            account.Balance = Math.Max(0, account.Balance - amountDecimal);
            _context.Accounts.Update(account);
            // 5. Conversie valutară (pentru scopuri interne - ex: raportări în RON)
            decimal convertedAmount = amountDecimal;
            int? exchangeRateId = null;

            var exchange = _context.ExchangeRates.FirstOrDefault(e => e.FromCurrency == ViewModel.Currency && e.ToCurrency == "RON");
            if (exchange != null)
            {
                convertedAmount = amountDecimal * (decimal)exchange.Rate;
                exchangeRateId = exchange.ExchangeRateId;
            }


            // 6. Căutăm dacă e plată către un serviciu
            var servicedPayment = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == ViewModel.ReceiverIBAN);
            int? servicedPaymentId = servicedPayment?.ServicedPaymentId;
            ViewModel.ServicedPaymentName = servicedPayment?.Bill_Name;

            // 7. Scădem suma din cont
            account.Balance -= amountDecimal;
            _context.Accounts.Update(account);

            // 8. Salvăm tranzacția
            var transaction = new Transaction
            {
                Sender = $"USER-{userId}",
                Receiver = ViewModel.ReceiverIBAN,
                Amount = (double)convertedAmount,
                Currency = ViewModel.Currency,
                DateTime = DateTime.Now,
                UserId = userId,
                ExchangeRateId = exchangeRateId,
                ServicedPaymentId = servicedPaymentId,
                Status = "Completed"
            };


            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // 9. Trimitem email de confirmare
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
            catch
            {
                // Ignorăm pentru demo
            }

            ViewModel.Status = "success";
            ViewModel.Message = "Payment completed and saved successfully.";
            return Page();

        }

    }

}