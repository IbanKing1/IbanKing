using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace IBanKing.Pages.Client
{
    public class PayServiceModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IEmailService _emailService;

        public PayServiceModel(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IEmailService emailService)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _emailService = emailService;
        }

        [BindProperty]
        public int SelectedAccountId { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public string SelectedCurrency { get; set; } = "RON";

        public List<Account> UserAccounts { get; set; } = new();
        public List<SelectListItem> CurrencyOptions { get; set; } = new();

        public ServicedPayment Service { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string serviceIBAN)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Login/Index");

            Service = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == serviceIBAN);
            if (Service == null)
                return RedirectToPage("/Client/MakePayment");

            UserAccounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            await LoadCurrenciesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string serviceIBAN)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Login/Index");

            Service = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == serviceIBAN);
            if (Service == null)
                return RedirectToPage("/Client/MakePayment");

            UserAccounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            await LoadCurrenciesAsync();

            var account = UserAccounts.FirstOrDefault(a => a.AccountId == SelectedAccountId);
            if (account == null)
            {
                ErrorMessage = "Selected account not found.";
                return Page();
            }

            decimal amountInAccountCurrency = Amount;

            if (SelectedCurrency != account.Currency)
            {
                double rate = await GetExchangeRateAsync(SelectedCurrency, account.Currency);
                amountInAccountCurrency = Amount * (decimal)rate;
            }

            if (account.Balance < amountInAccountCurrency)
            {
                ErrorMessage = $"Insufficient funds. Required: {amountInAccountCurrency:F2} {account.Currency}";
                return Page();
            }

            account.Balance -= amountInAccountCurrency;

            var transaction = new Transaction
            {
                Sender = account.IBAN,
                Receiver = Service.IBAN,
                Amount = (double)Amount,
                Currency = SelectedCurrency,
                DateTime = DateTime.Now,
                UserId = userId,
                Status = "Completed",
                ServicedPaymentId = Service.ServicedPaymentId
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // ✅ Trimite email de confirmare folosind .Name din User
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                await _emailService.SendPaymentConfirmationEmailAsync(
                    toEmail: user.Email,
                    userName: user.Name,
                    amount: Amount,
                    currency: SelectedCurrency,
                    receiver: Service.Bill_Name,
                    dateTime: DateTime.Now
                );
            }

            SuccessMessage = $"Payment of {Amount:F2} {SelectedCurrency} sent to {Service.Bill_Name}.";
            return Page();
        }

        private async Task<double> GetExchangeRateAsync(string from, string to)
        {
            try
            {
                if (from == to) return 1;

                var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(
                    $"https://api.frankfurter.app/latest?from={from}&to={to}");

                if (response != null && response.Rates.ContainsKey(to))
                    return response.Rates[to];

                return 1;
            }
            catch
            {
                return 1;
            }
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
                    "https://api.frankfurter.app/currencies");

                if (result != null)
                {
                    CurrencyOptions = result
                        .Select(kvp => new SelectListItem
                        {
                            Value = kvp.Key,
                            Text = $"{kvp.Key} - {kvp.Value}"
                        })
                        .OrderBy(c => c.Text)
                        .ToList();
                }
            }
            catch
            {
                CurrencyOptions = new List<SelectListItem>
                {
                    new("RON", "RON - Romanian Leu"),
                    new("EUR", "EUR - Euro"),
                    new("USD", "USD - US Dollar")
                };
            }
        }

        public class ExchangeRateApiResponse
        {
            public string Base { get; set; }
            public Dictionary<string, double> Rates { get; set; }
        }
    }
}
