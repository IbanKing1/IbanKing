using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
namespace IBanKing.Pages.Client
{
    public class HomeClientModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public HomeClientModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Account> Accounts { get; set; } = new();
        public string UserName { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }
            UserName = HttpContext.Session.GetString("UserName") ?? "Client";
            Accounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();
            return Page();
        }
        public IActionResult OnPostDelete(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == id && a.UserId == userId);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
