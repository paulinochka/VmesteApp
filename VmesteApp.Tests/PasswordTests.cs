using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VmesteApp.Security;

namespace VmesteApp.Tests
{
    [TestClass]
    public class PasswordTests
    {
        [TestMethod]
        public void HashPassword_SamePassword_ReturnsSameHash()
        {
            // Arrange
            string password = "my_secure_password";

            // Act
            string hash1 = PasswordHasher.HashPassword(password);
            string hash2 = PasswordHasher.HashPassword(password);

            // Assert
            Assert.AreEqual(hash1, hash2, "Хеши одинаковых паролей должны совпадать.");
        }
    }
}
