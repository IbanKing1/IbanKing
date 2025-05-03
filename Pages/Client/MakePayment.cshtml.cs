using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace IBanKing.Pages.Client
{
    public class MakePaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MakePaymentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ServicedPayment> Services { get; set; } = new();

        public void OnGet()
        {
            Services = _context.ServicedPayments.ToList();
        }

        public IActionResult OnPostTransfer()
        {
            return RedirectToPage("/Client/Step1");
        }
        public IActionResult OnPostPayService(string serviceIBAN)
        {
            return RedirectToPage("/Client/PayService", new { serviceIBAN });
        }
    }
}
