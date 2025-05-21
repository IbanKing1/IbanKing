using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.BankEmployee;
using IBanKing.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace UnitTests
{
    public class SearchByIdTests
    {
        private ApplicationDbContext _context;
        private IndexModel _pageModel;
        private Mock<INotificationService> _mockNotificationService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    Name = "Charlie Brown",
                    Role = "Client",
                    LastLog = DateTime.UtcNow,
                    Email = "charlie@example.com",
                    Password = "password123",
                    DateBirth = new DateTime(1990, 1, 1),
                    Gender = "Male",
                    Address = "123 Main St",
                    PhoneNumber = "555-1111",
                    TransactionLimit = "1000",
                    TransactionMaxAmount = "5000"
                },
                new User
                {
                    UserId = 2,
                    Name = "Alice Smith",
                    Role = "Client",
                    LastLog = DateTime.UtcNow,
                    Email = "alice@example.com",
                    Password = "password123",
                    DateBirth = new DateTime(1992, 5, 15),
                    Gender = "Female",
                    Address = "456 Oak Ave",
                    PhoneNumber = "555-2222",
                    TransactionLimit = "1000",
                    TransactionMaxAmount = "5000"
                },
                new User
                {
                    UserId = 3,
                    Name = "Bob Jones",
                    Role = "Client",
                    LastLog = DateTime.UtcNow,
                    Email = "bob@example.com",
                    Password = "password123",
                    DateBirth = new DateTime(1985, 8, 20),
                    Gender = "Male",
                    Address = "789 Pine Blvd",
                    PhoneNumber = "555-3333",
                    TransactionLimit = "1000",
                    TransactionMaxAmount = "5000"
                },
                new User
                {
                    UserId = 4,
                    Name = "Admin User",
                    Role = "Admin",
                    LastLog = DateTime.UtcNow,
                    Email = "admin@example.com",
                    Password = "admin123",
                    DateBirth = new DateTime(1980, 3, 10),
                    Gender = "Other",
                    Address = "101 Admin St",
                    PhoneNumber = "555-4444",
                    TransactionLimit = "5000",
                    TransactionMaxAmount = "10000"
                }
            );

            _context.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 1, Balance = 1000, IBAN = "RO01BANK0000000000000001", Currency = "EUR" },
                new Account { AccountId = 2, UserId = 2, Balance = 2000, IBAN = "RO01BANK0000000000000002", Currency = "EUR" },
                new Account { AccountId = 3, UserId = 3, Balance = 3000, IBAN = "RO01BANK0000000000000003", Currency = "EUR" }
            );

            _context.SaveChanges();

            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService.Setup(ns => ns.CreateInactivityNotification(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _pageModel = new IndexModel(_context, _mockNotificationService.Object)
            {
                Search = "2"
            };
        }

        [Test]
        public async Task OnGetAsync_SearchesById()
        {
            await _pageModel.OnGetAsync();

            var accounts = _pageModel.Accounts;

            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual(2, accounts[0].User.UserId);
            Assert.AreEqual("Alice Smith", accounts[0].User.Name);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}