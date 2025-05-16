using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

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
                query = FilterStatus == "Pending"
                    ? query.Where(t => t.Status.StartsWith("Pending"))
                    : query.Where(t => t.Status == FilterStatus);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchTermLower = SearchTerm.ToLower();

                if (DateTime.TryParseExact(SearchTerm, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var searchDate))
                {
                    query = query.Where(t =>
                        t.DateTime.Year == searchDate.Year &&
                        t.DateTime.Month == searchDate.Month &&
                        t.DateTime.Day == searchDate.Day);
                }
                else if (int.TryParse(SearchTerm, out int searchId))
                {
                    query = query.Where(t => t.TransactionId == searchId);
                }
               else if (double.TryParse(SearchTerm, NumberStyles.Any, CultureInfo.InvariantCulture, out double searchAmount))
                 {
                     query = query.Where(t => t.Amount == searchAmount);
                 }
                else
                {
                    query = query.Where(t =>
                        t.Sender.ToLower().Contains(searchTermLower) ||
                        t.Receiver.ToLower().Contains(searchTermLower) ||
                        t.Currency.ToLower().Contains(searchTermLower));
                }
            }

            UserTransactions = await query
                .OrderByDescending(t => t.DateTime)
                .ToListAsync();

            foreach (var tx in UserTransactions)
            {
                if (tx.Status.StartsWith("Pending"))
                {
                    tx.Status = "Pending";
                }
            }

            return Page();
        }
    }
}
