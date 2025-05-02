using IBanKing.Models;
using Microsoft.EntityFrameworkCore;
namespace IBanKing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);  

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ServicedPayment)
                .WithMany()
                .HasForeignKey(t => t.ServicedPaymentId)
                .OnDelete(DeleteBehavior.Restrict);  

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ExchangeRate)
                .WithMany()
                .HasForeignKey(t => t.ExchangeRateId)
                .OnDelete(DeleteBehavior.Restrict); 
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ServicedPayment> ServicedPayments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FavoriteCurrencyPair> FavoriteCurrencyPairs { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
    }
}
