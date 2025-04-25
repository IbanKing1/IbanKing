using IBanKing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IBanKing.Pages.ExchangeRate
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IFavoriteCurrencyService _favoriteCurrencyService;

        public IndexModel(IFavoriteCurrencyService favoriteCurrencyService)
        {
            _favoriteCurrencyService = favoriteCurrencyService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnGetFavoritesAsync(string baseCurrency)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return new JsonResult(new List<string> { "EUR", "USD", "GBP" });
            }

            var favorites = await _favoriteCurrencyService.GetFavoriteCurrenciesAsync(userId, baseCurrency);
            return new JsonResult(favorites);
        }

        public async Task<IActionResult> OnPostSaveFavoritesAsync([FromBody] SaveFavoritesRequest request)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            await _favoriteCurrencyService.SaveFavoriteCurrenciesAsync(userId, request.BaseCurrency, request.Currencies);
            return new OkResult();
        }
    }

    public class SaveFavoritesRequest
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public List<string> Currencies { get; set; } = new List<string>();
    }
}