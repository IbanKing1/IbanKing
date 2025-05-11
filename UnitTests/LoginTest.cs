using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Pages.Login;
using IBanKing.Utils;
using IBanKing.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests
{
    public class LoginTests
    {
        private ApplicationDbContext _context;
        private Mock<INotificationService> _notificationServiceMock;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            var dbName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            _context = new ApplicationDbContext(options);
            _notificationServiceMock = new Mock<INotificationService>();
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();

            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = "example@gmail.com",
                Password = PasswordHelper.HashPassword("1234"),
                DateBirth = new DateTime(1995, 1, 1),
                Gender = "Other",
                Address = "Test Street",
                PhoneNumber = "0712345678",
                Role = "Client",
                IsBlocked = false,
                FailedLoginAttempts = 0,
                TransactionLimit = "5000",
                TransactionMaxAmount = "10000",
                LastLog = DateTime.UtcNow.AddDays(-10),
                ProfilePicturePath = "/images/test.png"
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }


        [Test]
        public async Task Login_WithWrongUsername_ShouldShowError()
        {
            var loginPage = new IndexModel(_context, _notificationServiceMock.Object)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                LoginInput = new IndexModel.LoginInputModel
                {
                    Email = "notfound@gmail.com",
                    Password = "1234"
                }
            };

            var result = await loginPage.OnPostAsync();

            Assert.IsInstanceOf<PageResult>(result);
            Assert.AreEqual("Invalid email or account is blocked.", loginPage.ErrorMessage);
        }

        [Test]
        public async Task Login_WithWrongPassword_ShouldShowErrorAndIncrementFailedAttempts()
        {
            var loginPage = new IndexModel(_context, _notificationServiceMock.Object)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                LoginInput = new IndexModel.LoginInputModel
                {
                    Email = "example@gmail.com",
                    Password = "wrongpass"
                }
            };

            var result = await loginPage.OnPostAsync();

            Assert.IsInstanceOf<PageResult>(result);
            Assert.AreEqual("Incorrect password.", loginPage.ErrorMessage);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "example@gmail.com");
            Assert.AreEqual(1, user.FailedLoginAttempts);
        }

        [Test]
        public async Task Login_WithValidCredentials_ShouldRedirectAndSetSession()
        {
            var loginPage = new IndexModel(_context, _notificationServiceMock.Object)
            {
                PageContext = new PageContext { HttpContext = _httpContext },
                LoginInput = new IndexModel.LoginInputModel
                {
                    Email = "example@gmail.com",
                    Password = "1234"
                }
            };

            var result = await loginPage.OnPostAsync();

            Assert.IsInstanceOf<RedirectToPageResult>(result);
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("/Client/HomeClient", redirect.PageName);

            Assert.AreEqual("1", _httpContext.Session.GetString("UserId"));
            Assert.AreEqual("Client", _httpContext.Session.GetString("UserRole"));
            Assert.AreEqual("Test User", _httpContext.Session.GetString("UserName"));
            Assert.AreEqual("/images/test.png", _httpContext.Session.GetString("ProfilePicturePath"));
        }
    }

 
    public class TestSession : ISession
    {
        private Dictionary<string, byte[]> _storage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();
        public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);

        public void SetString(string key, string value) =>
            Set(key, System.Text.Encoding.UTF8.GetBytes(value));

        public string GetString(string key) =>
            TryGetValue(key, out var data) ? System.Text.Encoding.UTF8.GetString(data) : null;
    }
}
