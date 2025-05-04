using IBanKing.Models;
using IBanKing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Pages.Client
{
    public class NotificationsModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public NotificationsModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public List<Notification> Notifications { get; set; }

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            Notifications = await _notificationService.GetUserNotificationsAsync(userId, 50);
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            await _notificationService.MarkAllAsReadAsync(userId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _notificationService.DeleteAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            await _notificationService.DeleteAllAsync(userId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetUnreadCount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return new JsonResult(count);
        }
    }
}