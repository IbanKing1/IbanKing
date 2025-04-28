using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using IBanKing.Services;

namespace IBanKing.Pages.MakePayment
{
    public class Step3Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly INotificationService _notificationService;

        public Step3Model(ApplicationDbContext context, IHttpClientFactory httpClientFactory, INotificationService notificationService)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _notificationService = notificationService;
        }

        public Transaction ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData["SenderIBAN"] == null || TempData["ReceiverIBAN"] == null || TempData["Amount"] == null || TempData["Currency"] == null)
                return RedirectToPage("Step1");

            ViewModel.ReceiverIBAN = TempData["ReceiverIBAN"]!.ToString()!;
            ViewModel.Currency = TempData["Currency"]!.ToString()!;
            ViewModel.Amount = double.Parse(TempData["Amount"]!.ToString()!, CultureInfo.InvariantCulture);
            TempData.Keep();

            decimal originalAmount = Convert.ToDecimal(ViewModel.Amount, CultureInfo.InvariantCulture);

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
            var receiverAccount = _context.Accounts.FirstOrDefault(a => a.IBAN == ViewModel.ReceiverIBAN);

            if (senderAccount == null)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Invalid sender account.";
                return Page();
            }

            if (receiverAccount == null)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Receiver IBAN not found.";
                return Page();
            }

            decimal amountInSenderCurrency = originalAmount;
            if (ViewModel.Currency != senderAccount.Currency)
            {
                double rateToSenderCurrency = await GetExchangeRate(ViewModel.Currency, senderAccount.Currency);
                amountInSenderCurrency = originalAmount * (decimal)rateToSenderCurrency;
            }

            if (senderAccount.Balance < amountInSenderCurrency)
            {
                ViewModel.Status = "error";
                ViewModel.Message = "Insufficient funds.";
                return Page();
            }

            decimal amountInReceiverCurrency = amountInSenderCurrency;
            if (senderAccount.Currency != receiverAccount.Currency)
            {
                double rateToReceiverCurrency = await GetExchangeRate(senderAccount.Currency, receiverAccount.Currency);
                amountInReceiverCurrency = amountInSenderCurrency * (decimal)rateToReceiverCurrency;
            }

            senderAccount.Balance = Math.Max(0, senderAccount.Balance - amountInSenderCurrency);
            receiverAccount.Balance += amountInReceiverCurrency;

            _context.Accounts.Update(senderAccount);
            _context.Accounts.Update(receiverAccount);

            var transaction = new Transaction
            {
                Sender = senderIBAN,
                Receiver = ViewModel.ReceiverIBAN,
                Amount = (double)amountInReceiverCurrency,
                Currency = receiverAccount.Currency,
                DateTime = DateTime.Now,
                UserId = userId,
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _notificationService.CreatePaymentNotification(
                receiverAccount.UserId.ToString(),
                transaction.TransactionId,
                amountInReceiverCurrency,
                receiverAccount.Currency);

            ViewModel.Status = "success";
            ViewModel.Message = "Payment completed successfully.";
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

                if (response != null && response.Rates.ContainsKey(toCurrency))
                {
                    return response.Rates[toCurrency];
                }
                return 1;
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