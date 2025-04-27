using System.ComponentModel.DataAnnotations;

namespace IBanKing.Models
{
    public class FavoriteCurrencyPair
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string BaseCurrency { get; set; } = string.Empty;

        [Required]
        public string TargetCurrency { get; set; } = string.Empty;
    }
}