using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace IBanKing.Pages.MakePayment
{
    public class Step2Model : PageModel
    {
        public string ReceiverIBAN { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = "RON";

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Login/Index");

            if (TempData["ReceiverIBAN"] == null || TempData["Amount"] == null)
                return RedirectToPage("Step1");

            ReceiverIBAN = TempData["ReceiverIBAN"]!.ToString()!;
            Amount = double.Parse(TempData["Amount"]!.ToString()!, CultureInfo.InvariantCulture);
            Currency = TempData["Currency"]?.ToString() ?? "RON";

            TempData.Keep();
            return Page();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("Step3");
        }
    }
}
