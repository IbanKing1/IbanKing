using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Services.Interfaces
{
    public interface IFavoriteCurrencyService
    {
        Task<string> GetBaseCurrencyAsync(int userId);
        Task<List<string>> GetFavoritesAsync(int userId);
        Task AddFavoriteAsync(int userId, string targetCurrency);
        Task RemoveFavoriteAsync(int userId, string targetCurrency);
        Task SaveFavoritesAsync(int userId, string baseCurrency, List<string> currencies);
    }
}