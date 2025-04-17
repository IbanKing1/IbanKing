using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public string Message { get; set; }

        public DateTime DateTime { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
