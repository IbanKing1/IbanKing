using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IBanKing.Models;
using IBanKing.Data;
using Microsoft.AspNetCore.Http;
using IBanKing.Utils;

namespace IBanKing.Pages.Admin
{
    public class AddEmployeeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddEmployeeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BankEmployee NewEmployee { get; set; }

        public IActionResult OnPost()
        {
            // Check if the user is an Admin
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                // Redirect non-admin users to the Home page
                return RedirectToPage("/Home/Index");
            }

            // Create and add new Bank Employee to the database
            if (ModelState.IsValid)
            {
                _context.Users.Add(new User
                {
                    Name = NewEmployee.Name,
                    Email = NewEmployee.Email,
                    Password = PasswordHelper.HashPassword(NewEmployee.Password), // Hash the password
                    Role = "BankEmployee",
                    IsBlocked = false
                });

                _context.SaveChanges();
                return RedirectToPage("/Admin/Index"); // Redirect to Admin home
            }

            return Page();
        }
    }

    public class BankEmployee
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
