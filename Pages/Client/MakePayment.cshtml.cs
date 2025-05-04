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
        public MakePaymentModel(ApplicationDbContext context) => _context = context;
        public List<ServicedPayment> Services { get; set; } = new();
        public void OnGet()
        {
            Services = _context.ServicedPayments
                .OrderByDescending(s => s.IsHighPriority)
                .ThenBy(s => s.Bill_Name)
                .ToList();
        }
        public IActionResult OnPostTransfer() => RedirectToPage("/Client/Step1");
        public IActionResult OnPostPayService(string serviceIBAN)
            => RedirectToPage("/Client/PayService", new { serviceIBAN });
        public IActionResult OnPostTogglePriority(string serviceIBAN)
        {
            var service = _context.ServicedPayments.FirstOrDefault(s => s.IBAN == serviceIBAN);
            if (service != null)
            {
                service.IsHighPriority = !service.IsHighPriority;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}