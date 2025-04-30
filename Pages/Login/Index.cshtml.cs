using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using IBanKing.Models;
using IBanKing.Data;
using IBanKing.Utils;
using Microsoft.EntityFrameworkCore;
using IBanKing.Services;

namespace IBanKing.Pages.Login
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

        [BindProperty]
        public LoginInputModel LoginInput { get; set; }
        public List<User> InactiveUsers { get; set; }

        public string? ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == LoginInput.Email);

            if (user == null || user.IsBlocked)
            {
                Console.WriteLine("❌ Email does not exist OR account is blocked.");
                ErrorMessage = "Invalid email or account is blocked.";
                return Page();
            }

            var hashedInput = PasswordHelper.HashPassword(LoginInput.Password);

            Console.WriteLine($"🔐 Entered Email: {LoginInput.Email}");
            Console.WriteLine($"🔐 Hashed Password: {hashedInput}");
            Console.WriteLine($"🔐 Password from DB: {user.Password}");

            if (user.Password != hashedInput)
            {
                user.FailedLoginAttempts++;
                Console.WriteLine($"❌ Incorrect password. Attempts: {user.FailedLoginAttempts}");

                if (user.FailedLoginAttempts >= 4)
                {
                    user.IsBlocked = true;
                    Console.WriteLine("⛔ Account blocked after 4 failed attempts.");
                }

                await _context.SaveChangesAsync();
                ErrorMessage = "Incorrect password.";
                return Page();
            }

            // Reset failed attempts on successful login
            Console.WriteLine("✅ Password correct. Login successful.");
            user.FailedLoginAttempts = 0;

            // Check for inactivity
            var inactiveThreshold = DateTime.UtcNow.AddDays(-30);
            if (user.Role == "Client" && user.LastLog < inactiveThreshold)
            {
                var hasNotification = await _context.Notifications
                    .AnyAsync(n => n.UserId == user.UserId.ToString() &&
                                n.Type == "Inactivity" &&
                                n.CreatedAt > inactiveThreshold);

                if (!hasNotification)
                {
                    await _notificationService.CreateInactivityNotification(user.UserId.ToString());
                }
            }

            user.LastLog = DateTime.Now;
            await _context.SaveChangesAsync();

            // Store user session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Name);

            // Redirect user by role
            return user.Role switch
            {
                "Admin" => RedirectToPage("/Admin/Index"),
                "BankEmployee" => RedirectToPage("/BankEmployee/Index"),
                _ => RedirectToPage("/Client/HomeClient")
            };
        }
    }
}