using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreatePaymentRequestNotification(int userId, int transactionId, decimal amount)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "New Payment Received",
                Message = $"You have received a payment of {amount:C}. Transaction ID: {transactionId}",
                NotificationType = "PaymentRequest",
                TransactionId = transactionId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateInactivityNotification(int userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Account Inactivity Warning",
                Message = "You haven't logged in for a while. Please log in to keep your account active.",
                NotificationType = "Inactivity"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateTermsUpdateNotification(int userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Terms and Conditions Updated",
                Message = "Our terms and conditions have been updated. Please review them.",
                NotificationType = "TermsUpdate"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotifications(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Include(n => n.Transaction)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationsCount(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}