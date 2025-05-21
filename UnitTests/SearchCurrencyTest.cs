using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UnitTests
{
    public class SearchCurrencyTest
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
                new Account { AccountId = 2, UserId = 23, IBAN = "IBAN002", Currency = "USD" },
                new Account { AccountId = 3, UserId = 23, IBAN = "IBAN003", Currency = "RON" },
                new Account { AccountId = 4, UserId = 99, IBAN = "IBAN999", Currency = "GBP" }
            );

            _context.Transactions.AddRange(
                new Transaction { TransactionId = 1, Sender = "IBAN001", Receiver = "IBAN999", Amount = 100, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 2, Sender = "IBAN002", Receiver = "IBAN001", Amount = 200, Currency = "USD", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 3, Sender = "IBAN003", Receiver = "IBAN999", Amount = 300, Currency = "RON", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 4, Sender = "IBAN999", Receiver = "IBAN998", Amount = 400, Currency = "GBP", DateTime = DateTime.Now, Status = "Completed" }
            );

            _context.SaveChanges();
        }

        [Test]
        public async Task FiltersTransactionsByCurrency()
        {
            int userId = 23;
            string searchCurrency = "USD";

            var userIbans = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.IBAN)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => (userIbans.Contains(t.Sender) || userIbans.Contains(t.Receiver)) && t.Currency == searchCurrency)
                .ToListAsync();

            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual("USD", transactions[0].Currency);
            Assert.AreEqual(2, transactions[0].TransactionId);
        }

        [Test]
        public async Task ReturnsEmptyList_WhenNoTransactionsMatchCurrency()
        {
            int userId = 23;
            string searchCurrency = "GBP";

            var userIbans = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.IBAN)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => (userIbans.Contains(t.Sender) || userIbans.Contains(t.Receiver)) && t.Currency == searchCurrency)
                .ToListAsync();

            Assert.AreEqual(0, transactions.Count);
        }
    }
}
