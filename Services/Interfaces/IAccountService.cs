using IBanKing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Services.Interfaces
{
    public interface IAccountService
    {
        Task<List<Account>> GetUserAccountsAsync(int userId);
        Task<bool> ChangeAccountCurrencyAsync(int accountId, string newCurrency);
    }
}