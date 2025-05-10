using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace UnitTests
{
    public class ChangeAccountCurrencyTests
    {
        private ApplicationDbContext _dbContext;
        private AccountService _accountService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_RealAPI")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _accountService = new AccountService(_dbContext, new HttpClient());
        }

        private void SeedDatabase()
        {
            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = "test@example.com",
                Password = "hashed_password",
                DateBirth = new DateTime(2000, 1, 1),
                Gender = "Other",
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

            _dbContext.Accounts.Add(new Account
            {
                AccountId = 1,
                IBAN = "RO00TEST0000000000000000",
                Balance = 5000m,
                Currency = "RON",
                UserId = user.UserId,
                User = user
            });

            _dbContext.SaveChanges();
        }

        private async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            using var client = new HttpClient();
            var url = $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            if (json.RootElement.TryGetProperty("rates", out var ratesElement) &&
                ratesElement.TryGetProperty(toCurrency, out var rateElement))
            {
                return rateElement.GetDecimal();
            }

            throw new InvalidOperationException("Exchange rate not found in API response.");
        }

        [Test]
        public async Task ChangeAccountCurrencyAsync_ConvertsBalanceCorrectly()
        {
            // Arrange
            SeedDatabase();

            var accountId = 1;
            var initialCurrency = "RON";
            var targetCurrency = "EUR";
            var initialBalance = 5000m;

            var expectedRate = await GetExchangeRateAsync(initialCurrency, targetCurrency);
            var expectedBalance = initialBalance * expectedRate;
            Console.WriteLine($"Curs {initialCurrency} → {targetCurrency}: {expectedRate}");
            Console.WriteLine($"Sold asteptat dupa conversie: {expectedBalance}");
            // Act
            var result = await _accountService.ChangeAccountCurrencyAsync(accountId, targetCurrency);

            // Assert
            var updatedAccount = await _dbContext.Accounts.FindAsync(accountId);

            Assert.IsTrue(result);
            Assert.AreEqual(targetCurrency, updatedAccount.Currency);
            Assert.That(updatedAccount.Balance, Is.EqualTo(expectedBalance).Within(0.01m));
        }
    }
}
