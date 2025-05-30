﻿using System.Security.Cryptography;
using System.Text;

namespace IBanKing.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            string enteredHashed = HashPassword(enteredPassword);
            return enteredHashed == storedHashedPassword;
        }
    }
}
