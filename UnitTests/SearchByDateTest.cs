using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.Client;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace UnitTests
{
    public class SearchByDateTest
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
                new Account { AccountId = 2, UserId = 23, IBAN = "IBAN002", Currency = "USD" }
            );

            var date1 = new DateTime(2024, 5, 1, 10, 0, 0);
            var date2 = new DateTime(2024, 5, 2, 12, 0, 0);
            var date3 = new DateTime(2024, 5, 3, 15, 0, 0);

            _context.Transactions.AddRange(
                new Transaction { TransactionId = 1, Sender = "IBAN001", Receiver = "IBAN002", Amount = 100, Currency = "EUR", DateTime = date1, Status = "Completed" },
                new Transaction { TransactionId = 2, Sender = "IBAN002", Receiver = "IBAN001", Amount = 200, Currency = "USD", DateTime = date2, Status = "Pending" },
                new Transaction { TransactionId = 3, Sender = "IBAN001", Receiver = "IBAN002", Amount = 300, Currency = "EUR", DateTime = date3, Status = "Rejected" }
            );

            _context.SaveChanges();
        }

        [Test]
        public async Task OnGetAsync_FiltersTransactionsByDate()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();
            _httpContext.Session.SetString("UserId", "23");

            var searchDate = new DateTime(2024, 5, 2);
            string searchTerm = searchDate.ToString("dd.MM.yyyy"); 

            _pageModel = new TransactionsModel(_context)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                SearchTerm = searchTerm
            };

            var result = await _pageModel.OnGetAsync();

            Assert.IsInstanceOf<PageResult>(result);
            var transactions = _pageModel.UserTransactions;

            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual(2, transactions[0].TransactionId);
            Assert.AreEqual(searchDate.Date, transactions[0].DateTime.Date);
        }

        [Test]
        public async Task OnGetAsync_ReturnsEmptyList_WhenNoTransactionOnThatDate()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();
            _httpContext.Session.SetString("UserId", "23");

            string searchTerm = "10.05.2024"; 

            _pageModel = new TransactionsModel(_context)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                SearchTerm = searchTerm
            };

            var result = await _pageModel.OnGetAsync();

            Assert.IsInstanceOf<PageResult>(result);
            var transactions = _pageModel.UserTransactions;

            Assert.AreEqual(0, transactions.Count);
        }
    }
}
