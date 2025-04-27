using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class ExchangeRate
    {
        public int ExchangeRateId { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public double Rate { get; set; }
    }
}
