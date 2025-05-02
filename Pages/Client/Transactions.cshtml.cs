using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Pages.Client
{
    public class TransactionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TransactionsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Transaction> UserTransactions { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string FilterStatus { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login");
            }

            var userIBANs = _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.IBAN)
                .ToList();

            var query = _context.Transactions
                .Where(t => userIBANs.Contains(t.Sender) || userIBANs.Contains(t.Receiver));

            if (!string.IsNullOrEmpty(FilterStatus) && FilterStatus != "All")
            {
                query = query.Where(t => t.Status == FilterStatus);
            }

            UserTransactions = query
                .OrderByDescending(t => t.DateTime)
                .ToList();

            return Page();
        }
    }
}
