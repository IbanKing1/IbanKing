using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace IBanKing.Pages.Logout
{
    public class Index : PageModel
    {
        public IActionResult OnPost()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Prevent caching (back button)
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToPage("/Login/Index");
        }
    }
}
