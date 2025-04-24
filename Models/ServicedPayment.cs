using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class ServicedPayment
    {
        public int ServicedPaymentId { get; set; }

        public string Bill_Name { get; set; }

        public string Provider { get; set; }
        public string IBAN { get; set; }
          
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
