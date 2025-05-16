using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace UnitTests
{
    public class SearchByIBan
    {
        private ApplicationDbContext _context;
        private DefaultHttpContext _httpContext;
        private TransactionsModel _pageModel;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 23, IBAN = "IBAN001", Currency = "EUR" },
                new Account { AccountId = 2, UserId = 59, IBAN = "IBAN002", Currency = "USD" },
                new Account { AccountId = 3, UserId = 23, IBAN = "IBAN003", Currency = "RON" },
                new Account { AccountId = 4, UserId = 99, IBAN = "IBAN999", Currency = "GBP" },
                new Account { AccountId = 5, UserId = 46, IBAN = "IBAN789", Currency = "USD" },
                new Account { AccountId = 6, UserId = 78, IBAN = "IBAN008", Currency = "USD" }
            );

            _context.Transactions.AddRange(
                new Transaction { TransactionId = 1, Sender = "IBAN001", Receiver = "IBAN003", Amount = 100, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 2, Sender = "IBAN003", Receiver = "IBAN002", Amount = 200, Currency = "USD", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 3, Sender = "IBAN999", Receiver = "IBAN004", Amount = 300, Currency = "USD", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 4, Sender = "IBAN002", Receiver = "IBAN008", Amount = 300, Currency = "USD", DateTime = DateTime.Now, Status = "Completed" }

            );

            _context.SaveChanges();
        }

        [Test]
        public async Task OnGetAsync_FiltersTransactionsByUserIbans()
        {
            // Arrange
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();
            _httpContext.Session.SetString("UserId", "23");

            _pageModel = new TransactionsModel(_context)
            {
                PageContext = new PageContext { HttpContext = _httpContext }
            };

            // Act
            var result = await _pageModel.OnGetAsync();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            var transactions = _pageModel.UserTransactions;

            Assert.AreEqual(2, transactions.Count);
            Assert.IsTrue(transactions.Any(t => t.TransactionId == 1));
            Assert.IsTrue(transactions.Any(t => t.TransactionId == 2));
            Assert.IsFalse(transactions.Any(t => t.TransactionId == 3));
        }

        [Test]
        public async Task OnGetAsync_ReturnsEmptyList_WhenUserHasNoMatchingIbans()
        {
            // Arrange
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();
            _httpContext.Session.SetString("UserId", "46");

            _pageModel = new TransactionsModel(_context)
            {
                PageContext = new PageContext { HttpContext = _httpContext }
            };

            // Act
            var result = await _pageModel.OnGetAsync();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            var transactions = _pageModel.UserTransactions;

            Assert.AreEqual(0, transactions.Count);
        }
    }
}
