using System;
using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{ 
    public class TransactionsModel 
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        public string Status { get; set; }
    }
}
