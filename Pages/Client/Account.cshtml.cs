using IBanKing.Data;
using IBanKing.Models;
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

        public bool Success { get; set; } = false;

        public class EditInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            public string Address { get; set; }

            [Required]
            [RegularExpression(@"^\d{7,15}$", ErrorMessage = "Phone number must contain only digits (7-15 digits).")]
            public string PhoneNumber { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters.", MinimumLength = 8)]
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                var hashedPassword = IBanKing.Utils.PasswordHelper.HashPassword(Input.NewPassword);
                user.Password = hashedPassword;
            }

            user.Email = Input.Email;
            user.Address = Input.Address;
            user.PhoneNumber = Input.PhoneNumber;

            await _context.SaveChangesAsync();
            Success = true;

            return Page();
        }
    }
}
