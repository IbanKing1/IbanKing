using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace IBanKing.Pages.Admin
{
    public class AdminPageModel : PageModel
    {
        public IActionResult CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role != "Admin" ? RedirectToPage("/Home/Index") : null;
        }
    }
}