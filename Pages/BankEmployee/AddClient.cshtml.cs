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
        public InputModel Input { get; set; }

        public bool Success { get; set; } = false;

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime DateBirth { get; set; }

            [Required]
            public string Gender { get; set; }

            public string Address { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            int age = DateTime.Today.Year - Input.DateBirth.Year;
            if (Input.DateBirth.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                ModelState.AddModelError("Input.DateBirth", "User must be at least 18 years old.");
                return Page();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Email already in use.");
                return Page();
            }

            var user = new User
            {
                Name = Input.Name,
                Email = Input.Email,
                Password = PasswordHelper.HashPassword(Input.Password),
                DateBirth = Input.DateBirth,
                Gender = Input.Gender,
                Address = Input.Address,
                PhoneNumber = Input.PhoneNumber,
                Role = "Client",
                IsBlocked = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            Success = true;

            ModelState.Clear(); 
            Input = new InputModel(); 
            return Page();
        }

    }
}
