using Microsoft.AspNetCore.Mvc;

namespace IBanKing.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Transactions()
        {
            return View();
        }
    }
}
