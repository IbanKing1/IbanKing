using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace IBanKing.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                /* Redirect to login if session expired */
                return RedirectToPage("/Login/Index");
            }

            /* Prevent back button after logout */
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return Page();
        }
    }
}
