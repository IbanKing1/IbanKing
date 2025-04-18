using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace IBanKing.Pages.MakePayment
{
    public class Step2Model : PageModel
    {
        public string ReceiverIBAN { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public bool IsHighPriority { get; set; }

        public IActionResult OnGet()
        {
            if (TempData["ReceiverIBAN"] == null || TempData["Amount"] == null)
                return RedirectToPage("Index");

            ReceiverIBAN = TempData["ReceiverIBAN"].ToString();
            Amount = double.Parse(TempData["Amount"].ToString(), CultureInfo.InvariantCulture);
            Currency = TempData["Currency"]?.ToString() ?? "RON";
            IsHighPriority = TempData["IsHighPriority"]?.ToString() == "True";

            TempData.Keep(); // păstrează pentru Step3

            return Page();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("Step3");
        }
    }
}
