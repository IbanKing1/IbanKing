using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IBanKing.Pages.BankEmployee
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Account> Accounts { get; set; }
        public List<User> InactiveUsers { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinBalance { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Accounts
                .Include(a => a.User)
                .Where(a => a.User.Role == "Client")
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(a =>
                    a.User.Name.Contains(Search) ||
                    a.User.UserId.ToString() == Search);
            }

            if (MinBalance.HasValue)
            {
                query = query.Where(a => a.Balance >= MinBalance.Value);
            }

            query = SortBy switch
            {
                "balance_asc" => query.OrderBy(a => a.Balance),
                "balance_desc" => query.OrderByDescending(a => a.Balance),
                "name_asc" => query.OrderBy(a => a.User.Name),
                "name_desc" => query.OrderByDescending(a => a.User.Name),
                _ => query.OrderBy(a => a.User.UserId),
            };

            Accounts = await query.ToListAsync();

            // Logic to get inactive users (>30 days)
            var inactiveThreshold = DateTime.Now.AddDays(-30);

            InactiveUsers = await _context.Users
                .Where(u => u.Role == "Client" && u.LastLog < inactiveThreshold)
                .OrderBy(u => u.LastLog)
                .ToListAsync();
        }
    }
}
