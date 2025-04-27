using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace IBanKing.Pages.MakePayment
{
    public class Step1Model : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Step1Model(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string SenderIBAN { get; set; }

        [BindProperty]
        public string ReceiverIBAN { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public string Currency { get; set; } = "RON";

        public List<Account> UserAccounts { get; set; } = new();

        public IActionResult OnGet()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToPage("/Login/Index");

            UserAccounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            TempData["SenderIBAN"] = SenderIBAN;
            TempData["ReceiverIBAN"] = ReceiverIBAN;
            TempData["Amount"] = Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TempData["Currency"] = Currency;

            return RedirectToPage("Step2");
        }
    }
}
