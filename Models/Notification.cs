using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBanKing.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        [Required]
        public string NotificationType { get; set; } 

        public int? TransactionId { get; set; }

        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }
    }
}