using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Pages.Admin
{
    public class TermsAndConditionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public TermsAndConditionsModel(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [BindProperty]
        public TermsAndConditions Terms { get; set; }

        public async Task OnGetAsync()
        {
            Terms = await _context.TermsAndConditions.FirstOrDefaultAsync();
            if (Terms == null)
            {
                Terms = new TermsAndConditions();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingTerms = await _context.TermsAndConditions.FirstOrDefaultAsync();
            if (existingTerms == null)
            {
                Terms.LastUpdated = DateTime.UtcNow;
                Terms.UpdatedByUserId = 0;
                _context.TermsAndConditions.Add(Terms);
            }
            else
            {
                existingTerms.Content = Terms.Content;
                existingTerms.LastUpdated = DateTime.UtcNow;
                existingTerms.UpdatedByUserId = 0; 
                _context.TermsAndConditions.Update(existingTerms);
            }

            await _context.SaveChangesAsync();

            var allUsers = await _context.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                await _notificationService.CreateTermsUpdateNotification(user.UserId.ToString());
            }

            return RedirectToPage("/Admin/TermsAndConditions");
        }
    }
}