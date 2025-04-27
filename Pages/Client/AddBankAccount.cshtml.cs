using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;

namespace IBanKing.Pages.Client
{
    public class AddBankAccountModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddBankAccountModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Currency { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login/Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ErrorMessage = "Session expired. Please log in.";
                return Page();
            }

            var iban = GenerateIban();

            var newAccount = new Account
            {
                UserId = userId,
                Currency = Currency,
                IBAN = iban,
                Balance = 0
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            SuccessMessage = $"Account created successfully! IBAN: {iban}";
            return Page();
        }

        private string GenerateIban()
        {
            var random = new Random();
            var bankCode = "IBAN";
            var digits = new string(Enumerable.Range(0, 16)
                .Select(_ => random.Next(0, 10).ToString()[0]).ToArray());
            var checkDigits = random.Next(10, 99).ToString();
            return $"RO{checkDigits}{bankCode}{digits}";
        }
    }
}
