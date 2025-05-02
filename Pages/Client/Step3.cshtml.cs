using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IBanKing.Pages.MakePayment
{
    public class Step3Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public Step3Model(ApplicationDbContext context, IHttpClientFactory httpClientFactory, INotificationService notificationService, IEmailService emailService)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public Transaction ViewModel { get; set; } = new();
        public string ServicedPaymentName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData["SenderIBAN"] == null || TempData["ReceiverIBAN"] == null || TempData["Amount"] == null || TempData["Currency"] == null)
                return RedirectToPage("Step1");

            ViewModel.Receiver = TempData["ReceiverIBAN"]!.ToString()!;
            ViewModel.Currency = TempData["Currency"]!.ToString()!;
            ViewModel.Amount = double.Parse(TempData["Amount"]!.ToString()!, CultureInfo.InvariantCulture);
            TempData.Keep();

            var userIdStr = HttpContext.Session.GetString("UserId");
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

            string senderIBAN = TempData["SenderIBAN"]!.ToString()!;
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.IBAN == senderIBAN && a.UserId == userId);
            var receiverAccount = _context.Accounts.FirstOrDefault(a => a.IBAN == ViewModel.Receiver);

            if (senderAccount == null || receiverAccount == null)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Sender or Receiver account not found.";
                return Page();
            }

            decimal originalAmount = Convert.ToDecimal(ViewModel.Amount, CultureInfo.InvariantCulture);

            decimal amountInSenderCurrency = ViewModel.Currency == senderAccount.Currency
                ? originalAmount
                : Math.Round(originalAmount * (decimal)await GetExchangeRate(ViewModel.Currency, senderAccount.Currency), 2);

            if (senderAccount.Balance < amountInSenderCurrency)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Insufficient funds.";
                return Page();
            }

            double transactionMaxAmount = double.TryParse(user.TransactionMaxAmount, out var max) ? max : double.MaxValue;
            if ((double)amountInSenderCurrency > transactionMaxAmount)
            {
                ViewModel.Status = "error";
                ViewModel.Message = $"Transaction exceeds maximum allowed amount ({transactionMaxAmount}).";
                return Page();
            }

            int transactionLimit = int.TryParse(user.TransactionLimit, out var lim) ? lim : int.MaxValue;
            int todayCount = _context.Transactions.Count(t => t.UserId == userId && t.DateTime.Date == DateTime.Today);
            if (todayCount >= transactionLimit)
            {
                ViewModel.Status = "error";
                ViewModel.Message = $"Daily transaction limit of {transactionLimit} reached.";
                return Page();
            }

            decimal amountInReceiverCurrency = senderAccount.Currency == receiverAccount.Currency
                ? amountInSenderCurrency
                : Math.Round(amountInSenderCurrency * (decimal)await GetExchangeRate(senderAccount.Currency, receiverAccount.Currency), 2);

            var transaction = new Transaction
            {
                Sender = senderIBAN,
                Receiver = ViewModel.Receiver,
                Amount = Convert.ToDouble(amountInReceiverCurrency),
                Currency = receiverAccount.Currency,
                DateTime = DateTime.Now,
                UserId = userId,
                Status = $"Pending:{amountInSenderCurrency.ToString(CultureInfo.InvariantCulture)}:{senderAccount.Currency}"
            };

            var service = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == ViewModel.Receiver);
            if (service != null)
            {
                transaction.ServicedPaymentId = service.ServicedPaymentId;
                ServicedPaymentName = service.Bill_Name;
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _notificationService.CreatePaymentNotification(
                receiverAccount.UserId.ToString(),
                transaction.TransactionId,
                amountInReceiverCurrency,
                receiverAccount.Currency
            );

            // ✅ Send confirmation email
            await _emailService.SendPaymentConfirmationEmailAsync(
                toEmail: user.Email,
                userName: user.Name,
                amount: amountInReceiverCurrency,
                currency: receiverAccount.Currency,
                receiver: service?.Bill_Name ?? receiverAccount.IBAN,
                dateTime: transaction.DateTime
            );

            ViewModel.Status = "success";
            ViewModel.Message = "Payment submitted and awaiting bank employee approval.";
            return Page();
        }

        private async Task<double> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                if (fromCurrency == toCurrency)
                    return 1;

                var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(
                    $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}");

                return response != null && response.Rates.TryGetValue(toCurrency, out double rate)
                    ? rate
                    : 1;
            }
            catch
            {
                return 1;
            }
        }

        public class ExchangeRateApiResponse
        {
            public string Base { get; set; }
            public Dictionary<string, double> Rates { get; set; }
        }
    }
}
