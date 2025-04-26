using IBanKing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public interface INotificationService
    {
        Task CreatePaymentRequestNotification(int userId, int transactionId, decimal amount);
        Task CreateInactivityNotification(int userId);
        Task CreateTermsUpdateNotification(int userId);
        Task<List<Notification>> GetUserNotifications(int userId);
        Task<int> GetUnreadNotificationsCount(int userId);
        Task MarkAsRead(int notificationId);
    }
}