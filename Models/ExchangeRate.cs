using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class ExchangeRate
    {
        public int ExchangeRateId { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public double Rate { get; set; }
    }
}
