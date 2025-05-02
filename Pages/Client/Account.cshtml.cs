using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Pages.Client
{
    public class AccountModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AccountModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditInputModel Input { get; set; }

        public bool Success { get; set; }

        public class EditInputModel
        {
            [EmailAddress]
            public string? Email { get; set; }

            public string? Address { get; set; }

            [RegularExpression(@"^\d{7,15}$", ErrorMessage = "Phone number must contain only digits (7-15 digits).")]
            public string? PhoneNumber { get; set; }

            [DataType(DataType.Password)]
            public string? CurrentPassword { get; set; }

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 8)]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}:;'<>,.?\/~`-]).{8,}$",
                ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
            public string? NewPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            Input = new EditInputModel
            {
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.Email) && Input.Email != user.Email)
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email == Input.Email && u.UserId != userId);

                if (emailExists)
                {
                    ModelState.AddModelError("Input.Email", "This email is already in use.");
                    return Page();
                }

                user.Email = Input.Email;
            }

            if (!string.IsNullOrWhiteSpace(Input.Address))
            {
                user.Address = Input.Address;
            }

            if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                user.PhoneNumber = Input.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
                {
                    ModelState.AddModelError("Input.CurrentPassword", "You must enter your current password to set a new one.");
                    return Page();
                }

                if (!PasswordHelper.VerifyPassword(Input.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("Input.CurrentPassword", "Current password is incorrect.");
                    return Page();
                }

                // Prevent reuse of old password
                if (PasswordHelper.VerifyPassword(Input.NewPassword, user.Password))
                {
                    ModelState.AddModelError("Input.NewPassword", "New password must be different from the current password.");
                    return Page();
                }

                user.Password = PasswordHelper.HashPassword(Input.NewPassword);
            }

            await _context.SaveChangesAsync();
            Success = true;

            return Page();
        }
    }
}
