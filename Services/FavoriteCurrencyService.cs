using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;

namespace IBanKing.Services
{
    public interface IFavoriteCurrencyService
    {
        Task<List<string>> GetFavoriteCurrenciesAsync(int userId, string baseCurrency);
        Task SaveFavoriteCurrenciesAsync(int userId, string baseCurrency, List<string> currencies);
    }

    public class FavoriteCurrencyService : IFavoriteCurrencyService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteCurrencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetFavoriteCurrenciesAsync(int userId, string baseCurrency)
        {
            return await _context.FavoriteCurrencyPairs
                .Where(f => f.UserId == userId && f.BaseCurrency == baseCurrency)
                .Select(f => f.TargetCurrency)
                .ToListAsync();
        }

        public async Task SaveFavoriteCurrenciesAsync(int userId, string baseCurrency, List<string> currencies)
        {
            var existingFavorites = await _context.FavoriteCurrencyPairs
                .Where(f => f.UserId == userId && f.BaseCurrency == baseCurrency)
                .ToListAsync();

            _context.FavoriteCurrencyPairs.RemoveRange(existingFavorites);

            foreach (var currency in currencies)
            {
                _context.FavoriteCurrencyPairs.Add(new FavoriteCurrencyPair
                {
                    UserId = userId,
                    BaseCurrency = baseCurrency,
                    TargetCurrency = currency
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}