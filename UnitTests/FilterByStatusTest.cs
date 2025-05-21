using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace UnitTests
{
    public class TransactionsFilterByStatusTest
    {
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 23, IBAN = "IBAN001", Currency = "EUR" },
                new Account { AccountId = 2, UserId = 23, IBAN = "IBAN002", Currency = "USD" }
            );

            _context.Transactions.AddRange(
                new Transaction { TransactionId = 1, Sender = "IBAN001", Receiver = "IBAN002", Amount = 100, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 2, Sender = "IBAN002", Receiver = "IBAN001", Amount = 200, Currency = "USD", DateTime = DateTime.Now, Status = "Pending" },
                new Transaction { TransactionId = 3, Sender = "IBAN001", Receiver = "IBAN002", Amount = 150, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" }
            );

            _context.SaveChanges();
        }

        [Test]
        public async Task FilterTransactionsByStatus_Completed()
        {
            int userId = 23;
            string status = "Completed";

            var userIbans = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.IBAN)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => (userIbans.Contains(t.Sender) || userIbans.Contains(t.Receiver)) && t.Status == status)
                .ToListAsync();

            Assert.AreEqual(2, transactions.Count);
            Assert.True(transactions.All(t => t.Status == "Completed"));
        }

        [Test]
        public async Task FilterTransactionsByStatus_Pending()
        {
            int userId = 23;
            string status = "Pending";

            var userIbans = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.IBAN)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => (userIbans.Contains(t.Sender) || userIbans.Contains(t.Receiver)) && t.Status == status)
                .ToListAsync();

            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual("Pending", transactions[0].Status);
            Assert.AreEqual(2, transactions[0].TransactionId);
        }
    }
}
