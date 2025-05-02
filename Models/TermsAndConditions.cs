using System;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class TermsAndConditions
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime LastUpdated { get; set; }

        public int UpdatedByUserId { get; set; } 
    }
}