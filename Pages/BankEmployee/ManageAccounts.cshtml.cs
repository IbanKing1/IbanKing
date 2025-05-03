using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IBanKing.Pages.BankEmployee
{
    public class ManageAccountsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageAccountsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<User> Clients { get; set; }
        public IList<Account> Accounts { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Clients = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();

            Accounts = await _context.Accounts.ToListAsync();
        }

        public async Task<IActionResult> OnPostEditClientAsync(
            int UserId,
            string EditedName,
            string EditedTransactionLimit,
            string EditedTransactionMaxAmount)
        {
            var client = await _context.Users.FindAsync(UserId);
            if (client == null)
            {
                Message = "Client not found.";
                return RedirectToPage();
            }

            client.Name = EditedName;
            client.TransactionLimit = EditedTransactionLimit;
            client.TransactionMaxAmount = EditedTransactionMaxAmount;

            await _context.SaveChangesAsync();
            Message = "Client information updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditBalanceAsync(int AccountId, decimal EditedBalance)
        {
            var account = await _context.Accounts.FindAsync(AccountId);
            if (account == null)
            {
                Message = "Account not found.";
                return RedirectToPage();
            }

            account.Balance = EditedBalance;
            await _context.SaveChangesAsync();

            Message = "Account balance updated.";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostToggleBlockAsync(int id)
        {
            var client = await _context.Users.FindAsync(id);
            if (client == null)
            {
                Message = "Client not found.";
                return RedirectToPage();
            }

            client.IsBlocked = !client.IsBlocked;
            await _context.SaveChangesAsync();

            Message = client.IsBlocked ? "Client blocked." : "Client unblocked.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClientAsync(int id)
        {
            var client = await _context.Users.FindAsync(id);
            if (client == null)
            {
                Message = "Client not found.";
                return RedirectToPage();
            }

            _context.Users.Remove(client);
            await _context.SaveChangesAsync();

            Message = "Client deleted successfully.";
            return RedirectToPage();
        }
    }
}
