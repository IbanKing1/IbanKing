using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public class FavoriteCurrencyService : IFavoriteCurrencyService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteCurrencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetBaseCurrencyAsync(int userId)
        {
            var pair = await _context.FavoriteCurrencyPairs
                .Where(f => f.UserId == userId)
                .FirstOrDefaultAsync();

            return pair?.BaseCurrency ?? "RON";
        }

        public async Task<List<string>> GetFavoritesAsync(int userId)
        {
            return await _context.FavoriteCurrencyPairs
                .Where(f => f.UserId == userId)
                .Select(f => f.TargetCurrency)
                .ToListAsync();
        }

        public async Task AddFavoriteAsync(int userId, string targetCurrency)
        {
            var baseCurrency = await GetBaseCurrencyAsync(userId);
            var exists = await _context.FavoriteCurrencyPairs
                .AnyAsync(f => f.UserId == userId && f.TargetCurrency == targetCurrency);

            if (!exists)
            {
                _context.FavoriteCurrencyPairs.Add(new FavoriteCurrencyPair
                {
                    UserId = userId,
                    BaseCurrency = baseCurrency,
                    TargetCurrency = targetCurrency
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFavoriteAsync(int userId, string targetCurrency)
        {
            var favorite = await _context.FavoriteCurrencyPairs
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TargetCurrency == targetCurrency);

            if (favorite != null)
            {
                _context.FavoriteCurrencyPairs.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveFavoritesAsync(int userId, string baseCurrency, List<string> currencies)
        {
            var existingFavorites = await _context.FavoriteCurrencyPairs
                .Where(f => f.UserId == userId)
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