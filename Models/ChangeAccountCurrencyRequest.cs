namespace IBanKing.Models
{
    public class ChangeAccountCurrencyRequest
    {
        public int AccountId { get; set; }
        public string NewCurrency { get; set; }
    }
}