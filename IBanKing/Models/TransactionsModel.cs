namespace IBanKing.Models
{
    public class TransactionsModel
    {
        public int TransactionId { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
