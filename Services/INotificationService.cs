using IBanKing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public interface INotificationService
    {
        Task CreateAsync(Notification notification);
        Task CreatePaymentNotification(string userId, int transactionId, decimal amount, string currency);
        Task CreateInactivityNotification(string userId);
        Task CreateTermsUpdateNotification(string userId);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(int id);
        Task DeleteAllAsync(string userId);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
        Task<int> GetUnreadCountAsync(string userId);
    }
}