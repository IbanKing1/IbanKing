using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Pages.Admin
{
    public class AddEmployeeModel : AdminPageModel
    {
        private readonly ApplicationDbContext _context;

        public AddEmployeeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NewEmployeeInput Input { get; set; }
        public bool Success { get; set; }

        public class NewEmployeeInput
        {
            [Required]
            public string Name { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, MinLength(8)]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
                ErrorMessage = "Password must be at least 8 characters with one uppercase, one number, and one special character.")]
            public string Password { get; set; }

            [Required]
            public DateTime DateBirth { get; set; } = DateTime.Now.AddYears(-18);

            [Required]
            public string Gender { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
            public string PhoneNumber { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var employee = new User
            {
                Name = Input.Name,
                Email = Input.Email,
                Password = PasswordHelper.HashPassword(Input.Password),
                DateBirth = Input.DateBirth,
                Gender = Input.Gender,
                Address = Input.Address,
                PhoneNumber = Input.PhoneNumber,
                Role = "BankEmployee",
                IsBlocked = false,
                FailedLoginAttempts = 0,
                TransactionLimit = "0",
                TransactionMaxAmount = "0",
                LastLog = DateTime.Now
            };

            _context.Users.Add(employee);
            await _context.SaveChangesAsync();

            Success = true;
            ModelState.Clear();
            return Page();
        }
    }
}