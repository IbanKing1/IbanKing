using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace IBanKing.Pages.MakePayment
{
    public class Step1Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public Step1Model(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string SenderIBAN { get; set; }

        [BindProperty]
        public string ReceiverIBAN { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public string Currency { get; set; }

        public List<Account> UserAccounts { get; set; } = new();
        public List<SelectListItem> CurrencyOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToPage("/Login/Index");

            UserAccounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            await LoadCurrenciesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCurrenciesAsync();

            if (!ModelState.IsValid)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    UserAccounts = _context.Accounts
                        .Where(a => a.UserId == userId)
                        .ToList();
                }

                return Page();
            }

            TempData["SenderIBAN"] = SenderIBAN;
            TempData["ReceiverIBAN"] = ReceiverIBAN;
            TempData["Amount"] = Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TempData["Currency"] = Currency;

            return RedirectToPage("Step2");
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetFromJsonAsync<Dictionary<string, string>>("https://api.frankfurter.app/currencies");

                if (response != null)
                {
                    CurrencyOptions = response
                        .Select(c => new SelectListItem
                        {
                            Value = c.Key,
                            Text = $"{c.Key} - {c.Value}"
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
    }
}
