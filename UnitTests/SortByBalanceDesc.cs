using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.BankEmployee;
using IBanKing.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests
{
    public class SortByBalanceDescTests
    {
        private ApplicationDbContext _context;
        private IndexModel _pageModel;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var users = new List<User>
            {
                new User { UserId = 1, Name = "Alice", Email = "alice@example.com", Password = "Pass123!", Gender = "F", Address = "Addr1", PhoneNumber = "111", Role = "Client", TransactionLimit = "5", TransactionMaxAmount = "500", LastLog = DateTime.UtcNow },
                new User { UserId = 2, Name = "Bob", Email = "bob@example.com", Password = "Pass456!", Gender = "M", Address = "Addr2", PhoneNumber = "222", Role = "Client", TransactionLimit = "4", TransactionMaxAmount = "500", LastLog = DateTime.UtcNow },
                new User { UserId = 3, Name = "Charlie", Email = "charlie@example.com", Password = "Pass789!", Gender = "M", Address = "Addr3", PhoneNumber = "333", Role = "Client", TransactionLimit = "6", TransactionMaxAmount = "500", LastLog = DateTime.UtcNow }
            };

            var accounts = new List<Account>
            {
                new Account { AccountId = 1, UserId = 1, IBAN = "RO01BANK0000000000000001", Balance = 100, Currency = "EUR" },
                new Account { AccountId = 2, UserId = 2, IBAN = "RO01BANK0000000000000002", Balance = 300, Currency = "EUR" },
                new Account { AccountId = 3, UserId = 3, IBAN = "RO01BANK0000000000000003", Balance = 200, Currency = "EUR" }
            };

            _context.Users.AddRange(users);
            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();

            var notificationServiceMock = new Mock<INotificationService>();
            _pageModel = new IndexModel(_context, notificationServiceMock.Object)
            {
                SortBy = "balance_desc"
            };
        }

        [Test]
        public async Task OnGetAsync_SortsAccountsByBalanceDescending()
        {
            // Act
            await _pageModel.OnGetAsync();

            // Assert
            Assert.AreEqual(3, _pageModel.Accounts.Count);
            Assert.AreEqual(300, _pageModel.Accounts[0].Balance);
            Assert.AreEqual(200, _pageModel.Accounts[1].Balance);
            Assert.AreEqual(100, _pageModel.Accounts[2].Balance); 
        }
    }
}
