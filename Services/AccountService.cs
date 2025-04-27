using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public AccountService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<bool> ChangeAccountCurrencyAsync(int accountId, string newCurrency)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            if (account.Currency != newCurrency)
            {
                var rate = await GetExchangeRate(account.Currency, newCurrency);
                account.Balance = account.Balance * (decimal)rate;
                account.Currency = newCurrency;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        private async Task<double> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(
                    $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}");
                return response.Rates[toCurrency];
            }
            catch
            {
                return 1;
            }
        }

        public async Task<List<Account>> GetUserAccountsAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        private class ExchangeRateApiResponse
        {
            public Dictionary<string, double> Rates { get; set; }
        }
    }
}