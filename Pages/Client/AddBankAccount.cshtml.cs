using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Pages.Client
{
    public class AddBankAccountModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AddBankAccountModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Currency { get; set; }

        public List<SelectListItem> CurrencyOptions { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login/Index");
            }

            await LoadCurrenciesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCurrenciesAsync();

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ErrorMessage = "Session expired. Please log in.";
                return Page();
            }

            if (string.IsNullOrEmpty(Currency))
            {
                ErrorMessage = "Please select a currency.";
                return Page();
            }

            var iban = GenerateIban();

            var newAccount = new Account
            {
                UserId = userId,
                Currency = Currency,
                IBAN = iban,
                Balance = 0
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            SuccessMessage = $"Account created successfully! IBAN: {iban}";
            Currency = string.Empty;
            return Page();
        }

        private string GenerateIban()
        {
            var random = new Random();
            var bankCode = "IBAN";
            var digits = new string(Enumerable.Range(0, 16)
                .Select(_ => random.Next(0, 10).ToString()[0]).ToArray());
            var checkDigits = random.Next(10, 99).ToString();
            return $"RO{checkDigits}{bankCode}{digits}";
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var result = await client.GetFromJsonAsync<Dictionary<string, string>>("https://api.frankfurter.app/currencies");

                if (result != null)
                {
                    CurrencyOptions = result
                        .Select(kvp => new SelectListItem
                        {
                            Value = kvp.Key,
                            Text = $"{kvp.Key} - {kvp.Value}"
                        })
                        .OrderBy(item => item.Text)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load currencies: " + ex.Message;
            }
        }
    }
}
