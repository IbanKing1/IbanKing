using IBanKing.Models;
using IBanKing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Pages.ExchangeRate
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IFavoriteCurrencyService _favoriteCurrencyService;
        private readonly IAccountService _accountService;

        public IndexModel(
            IFavoriteCurrencyService favoriteCurrencyService,
            IAccountService accountService)
        {
            _favoriteCurrencyService = favoriteCurrencyService;
            _accountService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnGetBaseCurrencyAsync()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return new JsonResult("RON");
            }

            var baseCurrency = await _favoriteCurrencyService.GetBaseCurrencyAsync(userId);
            return new JsonResult(baseCurrency ?? "RON");
        }

        public async Task<IActionResult> OnGetFavoritesAsync()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return new JsonResult(new List<string>());
            }

            var favorites = await _favoriteCurrencyService.GetFavoritesAsync(userId);
            return new JsonResult(favorites);
        }

        public async Task<IActionResult> OnPostAddFavoriteAsync([FromBody] AddFavoriteRequest request)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            await _favoriteCurrencyService.AddFavoriteAsync(userId, request.TargetCurrency);
            return new OkResult();
        }

        public async Task<IActionResult> OnPostRemoveFavoriteAsync([FromBody] RemoveFavoriteRequest request)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            await _favoriteCurrencyService.RemoveFavoriteAsync(userId, request.TargetCurrency);
            return new OkResult();
        }

        public async Task<IActionResult> OnPostSaveFavoritesAsync([FromBody] SaveFavoritesRequest request)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            await _favoriteCurrencyService.SaveFavoritesAsync(userId, request.BaseCurrency, request.Currencies);
            return new OkResult();
        }

        public async Task<IActionResult> OnGetUserAccountsAsync()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            var accounts = await _accountService.GetUserAccountsAsync(userId);
            return new JsonResult(accounts);
        }

        public async Task<IActionResult> OnPostChangeAccountCurrencyAsync([FromBody] ChangeAccountCurrencyRequest request)
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                return Unauthorized();
            }

            var success = await _accountService.ChangeAccountCurrencyAsync(request.AccountId, request.NewCurrency);
            return success ? new OkResult() : BadRequest();
        }
        public class AddFavoriteRequest
        {
            public string TargetCurrency { get; set; }
        }

        public class RemoveFavoriteRequest
        {
            public string TargetCurrency { get; set; }
        }

        public class SaveFavoritesRequest
        {
            public string BaseCurrency { get; set; }
            public List<string> Currencies { get; set; }
        }
    }
}