using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IBanKing.Pages.Admin
{
    public class ManageEmployeesModel : AdminPageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageEmployeesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> Employees { get; set; }
        [TempData] public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Employees = await _context.Users
                .Where(u => u.Role == "BankEmployee")
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(
            int userId, string name, string email,
            string phoneNumber, string address)
        {
            var employee = await _context.Users.FindAsync(userId);
            if (employee == null)
            {
                Message = "Employee not found.";
                return RedirectToPage();
            }

            employee.Name = name;
            employee.Email = email;
            employee.PhoneNumber = phoneNumber;
            employee.Address = address;

            await _context.SaveChangesAsync();
            Message = "Employee updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(int userId)
        {
            var employee = await _context.Users.FindAsync(userId);
            if (employee == null)
            {
                Message = "Employee not found.";
                return RedirectToPage();
            }

            employee.IsBlocked = !employee.IsBlocked;
            await _context.SaveChangesAsync();

            Message = employee.IsBlocked ? "Employee blocked." : "Employee unblocked.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(int userId)
        {
            var employee = await _context.Users.FindAsync(userId);
            if (employee == null)
            {
                Message = "Employee not found.";
                return RedirectToPage();
            }

            _context.Users.Remove(employee);
            await _context.SaveChangesAsync();

            Message = "Employee deleted successfully.";
            return RedirectToPage();
        }
    }
}