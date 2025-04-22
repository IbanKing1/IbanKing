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

        public List<User> Clients { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            Clients = await _context.Users
                .Where(u => u.Role == "Client")
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Client")
            {
                return NotFound();
            }

            user.IsBlocked = !user.IsBlocked;
            await _context.SaveChangesAsync();

            Message = $"Client {user.Name} has been {(user.IsBlocked ? "blocked" : "unblocked")}.";

            return RedirectToPage();
        }

        [BindProperty]
        public string? EditedName { get; set; }

        [BindProperty]
        public int EditUserId { get; set; }

        public async Task<IActionResult> OnPostEditNameAsync()
        {
            var user = await _context.Users.FindAsync(EditUserId);
            if (user == null || user.Role != "Client")
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(EditedName))
            {
                user.Name = EditedName;
                await _context.SaveChangesAsync();
                Message = $"Name updated for client with email: {user.Email}";
            }

            return RedirectToPage();
        }
    }
}
