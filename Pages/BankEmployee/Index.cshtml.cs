using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Pages.BankEmployee
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public IndexModel(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

            var inactiveThreshold = DateTime.Now.AddDays(-30);
            InactiveUsers = await _context.Users
                .Where(u => u.Role == "Client" && u.LastLog < inactiveThreshold)
                .OrderBy(u => u.LastLog)
                .ToListAsync();

            foreach (var user in InactiveUsers)
            {
                var hasNotification = await _context.Notifications
                    .AnyAsync(n => n.UserId == user.UserId &&
                                  n.NotificationType == "Inactivity" &&
                                  n.CreatedAt > inactiveThreshold);

                if (!hasNotification)
                {
                    await _notificationService.CreateInactivityNotification(user.UserId);
                }
            }
        }
    }
}