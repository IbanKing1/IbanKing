using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Pages.ExchangeRate
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public List<Models.ExchangeRate> ExchangeRates { get; set; } = new();
        public Dictionary<string, decimal> LiveRates { get; set; } = new();
        public string BaseCurrency { get; set; } = "EUR";
        public List<Models.Account> UserAccounts { get; set; } = new();

        public IndexModel(ApplicationDbContext context,
                        IConfiguration configuration,
                        IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task OnGetAsync()
        {
            ExchangeRates = await _context.ExchangeRates.ToListAsync();

            var apiKey = _configuration["ExchangeRateApi:ApiKey"];
            var baseUrl = _configuration["ExchangeRateApi:BaseUrl"];
            var response = await _httpClient.GetAsync($"{baseUrl}{apiKey}/latest/{BaseCurrency}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExchangeRateApiResponse>(content);
                LiveRates = result.conversion_rates;
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                UserAccounts = await _context.Accounts
                    .Where(a => a.UserId == userId.Value)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostConvertAsync(
            string fromCurrency,
            string toCurrency,
            decimal amount)
        {
            try
            {
                var apiKey = _configuration["ExchangeRateApi:ApiKey"];
                var baseUrl = _configuration["ExchangeRateApi:BaseUrl"];
                var response = await _httpClient.GetAsync(
                    $"{baseUrl}{apiKey}/pair/{fromCurrency}/{toCurrency}/{amount}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ConversionResult>(content);
                    return new JsonResult(result);
                }
                return new JsonResult(new { error = "API request failed" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        private class ExchangeRateApiResponse
        {
            public string base_code { get; set; }
            public Dictionary<string, decimal> conversion_rates { get; set; }
        }

        private class ConversionResult
        {
            public string base_code { get; set; }
            public string target_code { get; set; }
            public decimal conversion_rate { get; set; }
            public decimal conversion_result { get; set; }
        }
    }
}