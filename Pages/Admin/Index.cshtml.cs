using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IBanKing.Pages.Admin
{
    public class IndexModel : AdminPageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int EmployeeCount { get; set; }
        public int BlockedEmployees { get; set; }
        public List<User> RecentEmployees { get; set; }

        public async Task OnGetAsync()
        {
            EmployeeCount = await _context.Users.CountAsync(u => u.Role == "BankEmployee");
            BlockedEmployees = await _context.Users.CountAsync(u => u.Role == "BankEmployee" && u.IsBlocked);
            RecentEmployees = await _context.Users
                .Where(u => u.Role == "BankEmployee")
                .OrderByDescending(u => u.UserId)
                .Take(5)
                .ToListAsync();
        }
    }
}