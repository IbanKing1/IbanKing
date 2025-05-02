using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using IBanKing.Services;

namespace IBanKing.Pages.Admin
{
    public class TermsAndConditionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public TermsAndConditionsModel(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [BindProperty]
        public TermsAndConditions Terms { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Terms = await _context.TermsAndConditions
                .OrderByDescending(t => t.LastUpdated)
                .FirstOrDefaultAsync();

            if (Terms == null)
            {
                Terms = new TermsAndConditions
                {
                    Content = "Enter your terms and conditions here...",
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

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
            {
                return userId;
            }
            return 0;
        }
    }
}