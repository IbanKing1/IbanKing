using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBanKing.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        [Required]
        public string Type { get; set; }

        public string ActionUrl { get; set; }

        public int? TransactionId { get; set; }

        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }

        [NotMapped]
        public string IconClass => GetIconClass();

        [NotMapped]
        public string TimeAgo => GetTimeAgo();

        private string GetIconClass()
        {
            return Type switch
            {
                "Payment" => "fas fa-money-bill-wave",
                "Inactivity" => "fas fa-clock",
                "Security" => "fas fa-shield-alt",
                "Account" => "fas fa-user",
                _ => "fas fa-bell"
            };
        }

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;
            return timeSpan.TotalSeconds < 60 ? "Just now" :
                   timeSpan.TotalMinutes < 60 ? $"{(int)timeSpan.TotalMinutes}m ago" :
                   timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours}h ago" :
                   $"{(int)timeSpan.TotalDays}d ago";
        }
    }
}