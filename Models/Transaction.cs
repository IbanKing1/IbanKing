using IBanKing.Pages;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBanKing.Models
{ 
    public class Transaction 
    {

        public int TransactionId { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }

        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        public string Currency { get; set; }

        public int UserId { get; set; }
       
        public int ServicedPaymentId { get; set; }
     
        public int ExchangeRateId { get; set; }
        public User User { get; set; }
        public ServicedPayment ServicedPayment { get; set; }

        public ExchangeRate ExchangeRate { get; set; }
      
   
    }
}
