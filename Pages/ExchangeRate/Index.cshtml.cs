using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IBanKing.Pages.ExchangeRate
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<JsonResult> OnGetLiveRatesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiKey = _configuration["ExchangeRateApi:ApiKey"];
            var response = await client.GetAsync($"https://v6.exchangerate-api.com/v6/{apiKey}/latest/USD");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new JsonResult(JsonSerializer.Deserialize<ExchangeRateApiResponse>(content));
            }

            return new JsonResult(new { error = "Failed to fetch rates" });
        }

        private class ExchangeRateApiResponse
        {
            public string result { get; set; }
            public string base_code { get; set; }
            public Dictionary<string, decimal> conversion_rates { get; set; }
        }
    }
}