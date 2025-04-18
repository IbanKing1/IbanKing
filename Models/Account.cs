namespace IBanKing.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } 
        public double Balance { get; set; }
        public string Currency { get; set; }

        public User User { get; set; }
    }
}
