using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ChangeAccountCurrencyAsync(int accountId, string newCurrency)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            account.Currency = newCurrency;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Account>> GetUserAccountsAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }
    }
}