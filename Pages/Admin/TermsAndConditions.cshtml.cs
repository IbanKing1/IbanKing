using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using IBanKing.Services;
using Microsoft.AspNetCore.Http;

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

        public string DefaultTerms => @"TERMS AND CONDITIONS OF USE

1. ACCEPTANCE OF TERMS
By accessing or using IBanKing services, you agree to be bound by these Terms.

2. ACCOUNT REGISTRATION
2.1. You must provide accurate and complete information
2.2. You are responsible for maintaining confidentiality of your credentials

3. SERVICES
3.1. IBanKing provides online banking services including:
- Account management
- Funds transfer
- Payment processing
- Financial analytics

4. USER OBLIGATIONS
4.1. Lawful use only
4.2. No fraudulent activity
4.3. Compliance with all applicable laws

5. PRIVACY POLICY
Your personal data will be processed according to our Privacy Policy.

6. LIMITATION OF LIABILITY
IBanKing shall not be liable for:
- Indirect damages
- Loss of profits
- Unauthorized account access due to user negligence

7. AMENDMENTS
We reserve the right to modify these Terms at any time.

8. GOVERNING LAW
These Terms shall be governed by Romanian law.

9. CONTACT
For questions, contact ibankingfamily@gmail.com";

        public async Task<IActionResult> OnGetAsync()
        {
            Terms = await _context.TermsAndConditions
                .OrderByDescending(t => t.LastUpdated)
                .FirstOrDefaultAsync();

            if (Terms == null)
            {
                Terms = new TermsAndConditions
                {
                    Content = DefaultTerms,
                    LastUpdated = DateTime.Now,
                    UpdatedByUserId = GetCurrentUserId()
                };
                _context.TermsAndConditions.Add(Terms);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existingTerms = await _context.TermsAndConditions
                .OrderByDescending(t => t.LastUpdated)
                .FirstOrDefaultAsync();

            if (existingTerms == null)
            {
                existingTerms = new TermsAndConditions();
                _context.TermsAndConditions.Add(existingTerms);
            }

            existingTerms.Content = Terms.Content;
            existingTerms.LastUpdated = DateTime.Now;
            existingTerms.UpdatedByUserId = GetCurrentUserId();

            await _context.SaveChangesAsync();
            await SendNotificationsToAllUsers();

            TempData["SuccessMessage"] = "Terms and conditions updated successfully!";
            return RedirectToPage();
        }

        private async Task SendNotificationsToAllUsers()
        {
            var allUserIds = await _context.Users
                .Where(u => !u.IsBlocked)
                .Select(u => u.UserId.ToString())
                .ToListAsync();

            foreach (var userId in allUserIds)
            {
                await _notificationService.CreateTermsUpdateNotification(userId);
            }
        }

        private int GetCurrentUserId()
        {
            if (int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
                return userId;
            return 0;
        }
    }
}