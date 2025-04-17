﻿using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class User
    {
        public int UserId { get; set; } 

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime DateBirth { get; set; }

        public string Gender { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Role { get; set; }

        public bool IsBlocked { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
    }
}
