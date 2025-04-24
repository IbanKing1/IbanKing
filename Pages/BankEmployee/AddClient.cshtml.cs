using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Pages.BankEmployee
{
    public class AddClientModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddClientModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NewClientInput Input { get; set; }

        public bool Success { get; set; } = false;

        public class NewClientInput
        {
            [Required]
            public string Name { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, MinLength(8)]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
                ErrorMessage = "Password must be at least 8 characters and include one uppercase letter, one number, and one special character.")]
            public string Password { get; set; }

            [Required]
            public DateTime DateBirth { get; set; }

            [Required]
            public string Gender { get; set; }

            public string Address { get; set; }

            [Required]
            [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var hashedPassword = PasswordHelper.HashPassword(Input.Password);

            var newClient = new User
            {
                Name = Input.Name,
                Email = Input.Email,
                Password = hashedPassword,
                DateBirth = Input.DateBirth,
                Gender = Input.Gender,
                Address = Input.Address,
                PhoneNumber = Input.PhoneNumber,
                Role = "Client",
                IsBlocked = false,
                FailedLoginAttempts = 0,
                TransactionLimit = "0",
                TransactionMaxAmount = "0",
                LastLog = DateTime.Now
            };

            _context.Users.Add(newClient);
            await _context.SaveChangesAsync();

            Success = true;
            ModelState.Clear();
            return Page();
        }
    }
}
