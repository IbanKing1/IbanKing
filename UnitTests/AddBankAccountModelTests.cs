using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace UnitTests
{
    [TestFixture]
    public class AddBankAccountModelTests
    {
        private ApplicationDbContext _dbContext;
        private AddBankAccountModel _model;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpContext> _mockHttpContext;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_AddBankAccount_" + Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            _mockHttpContext = new Mock<HttpContext>();

            _model = new AddBankAccountModel(_dbContext, _mockHttpClientFactory.Object);

            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = "test@example.com",
                Password = "hashed_password",
                DateBirth = new DateTime(2000, 1, 1),
                Gender = "Male",
                Address = "123 Test St",
                PhoneNumber = "0700000000",
                Role = "Client",
                IsBlocked = false,
                FailedLoginAttempts = 0,
                TransactionLimit = "5000",
                TransactionMaxAmount = "10000",
                LastLog = DateTime.Now,
                ProfilePicturePath = "/images/user.png"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task CreateAccount_WithValidData_AddsAccountToDatabase()
        {
            SetupUserSession("1");
            _model.Currency = "EUR";
            int initialAccountCount = _dbContext.Accounts.Count();

            await _model.OnPostAsync();

            int newAccountCount = _dbContext.Accounts.Count();
            Assert.AreEqual(initialAccountCount + 1, newAccountCount);

            var account = _dbContext.Accounts.FirstOrDefault();
            Assert.IsNotNull(account);
            Assert.AreEqual("EUR", account.Currency);
            Assert.AreEqual(0, account.Balance);
            Assert.AreEqual(1, account.UserId);
            Assert.IsNotNull(account.IBAN);
            Assert.IsTrue(account.IBAN.StartsWith("RO"));
            Assert.IsNotNull(_model.SuccessMessage);
        }

        [Test]
        public async Task CreateAccount_WithNoUserSession_SetsErrorMessage()
        {
            SetupUserSession(null);
            _model.Currency = "EUR";

            await _model.OnPostAsync();

            Assert.IsNotNull(_model.ErrorMessage);
            Assert.IsTrue(_model.ErrorMessage.Contains("Session expired"));
        }

        [Test]
        public async Task CreateAccount_WithNoCurrency_SetsErrorMessage()
        {
            SetupUserSession("1");
            _model.Currency = "";

            await _model.OnPostAsync();

            Assert.IsNotNull(_model.ErrorMessage);
            Assert.IsTrue(_model.ErrorMessage.Contains("select a currency"));
        }

        private void SetupUserSession(string userId)
        {
            var sessionDict = new Dictionary<string, byte[]>();

            if (userId != null)
            {
                sessionDict["UserId"] = Encoding.UTF8.GetBytes(userId);
            }

            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("UserId", out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => {
                    var success = sessionDict.TryGetValue(key, out var result);
                    value = result;
                    return success;
                });

            _mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var pageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = _mockHttpContext.Object
            };
            _model.PageContext = pageContext;
        }
    }
}