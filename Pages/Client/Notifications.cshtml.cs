using IBanKing.Models;
using IBanKing.Services;
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
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            Notifications = await _notificationService.GetUserNotifications(userId);
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            await _notificationService.MarkAsRead(id);
            return RedirectToPage();
        }
    }
}