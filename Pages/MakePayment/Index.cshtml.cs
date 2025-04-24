using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IBanKing.Pages.MakePayment
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string ReceiverIBAN { get; set; }

        [BindProperty]
        public double Amount { get; set; }

        [BindProperty]
        public string Currency { get; set; }

        [BindProperty]
        public bool IsHighPriority { get; set; }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Stocăm temporar datele și mergem la Step 2
            TempData["ReceiverIBAN"] = ReceiverIBAN;
            TempData["Amount"] = Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TempData["Currency"] = Currency;
            TempData["IsHighPriority"] = IsHighPriority.ToString();


            return RedirectToPage("Step2");
        }
    }
}
