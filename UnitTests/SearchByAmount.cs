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
    public class SearchByAmountTests
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

            _context.Accounts.Add(new Account { UserId = 23, IBAN = "IBAN123", Currency = "EUR" });

            _context.Transactions.AddRange(
                new Transaction { TransactionId = 1, Sender = "IBAN123", Receiver = "IBAN999", Amount = 200.00, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 2, Sender = "IBAN123", Receiver = "IBAN999", Amount = 150.00, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" },
                new Transaction { TransactionId = 3, Sender = "IBAN123", Receiver = "IBAN999", Amount = 200.01, Currency = "EUR", DateTime = DateTime.Now, Status = "Completed" }
            );

            _context.SaveChanges();
        }

        [Test]
        public async Task OnGetAsync_FiltersTransactionsByAmount()
        {
            // Arrange
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();
            _httpContext.Session.SetString("UserId", "23");

            _pageModel = new TransactionsModel(_context)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                SearchTerm = "200.00"
            };

            // Act
            var result = await _pageModel.OnGetAsync();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            var transactions = _pageModel.UserTransactions;

            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual(1, transactions[0].TransactionId);
            Assert.AreEqual(200.00, transactions[0].Amount);
        }
    }
}
