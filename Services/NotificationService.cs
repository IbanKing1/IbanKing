using IBanKing.Data;
using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task CreateAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreatePaymentNotification(string userId, int transactionId, decimal amount, string currency)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            var notification = new Notification
            {
                UserId = userId,
                Title = "Payment Received",
                Message = $"You've received a payment of {amount.ToString("N2")} {currency}",
                Type = "Payment",
                TransactionId = transactionId,
                ActionUrl = $"/Client/Transactions",
                Transaction = transaction
            };
            await CreateAsync(notification);
        }

        public async Task CreateInactivityNotification(string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Inactivity Warning",
                Message = "You haven't logged in for over 30 days",
                Type = "Inactivity",
                ActionUrl = "/Client/Account"
            };
            await CreateAsync(notification);
        }

        public async Task CreateTermsUpdateNotification(string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Terms and Conditions Updated",
                Message = "The terms and conditions have been updated. Please review them.",
                Type = "Account",
                ActionUrl = "/Client/TermsAndConditions"
            };
            await CreateAsync(notification);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10)
        {
            return await _context.Notifications
                .Include(n => n.Transaction)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}