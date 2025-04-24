using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string IBAN { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }

}