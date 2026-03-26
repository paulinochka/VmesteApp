using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;


namespace VmesteApp.Tests
{
    [TestClass]
    public class RegistrationTests
    {
        private RegistrationValidator _validator = new RegistrationValidator();

        [TestMethod]
        public async Task Validate_EmptyName_ReturnsError()
        {
            // Arrange
            string name = "";

            // Act
            var result = await _validator.ValidateAsync(name, "test@mail.ru",
                "+7 (999) 000-00-00", "123456", "Глава семьи", "");

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Введите ваше полное имя", result.Error);
        }

        [TestMethod]
        public async Task Validate_InvalidEmail_ReturnsError()
        {
            // Arrange
            string email = "wrong-email";

            // Act
            var result = await _validator.ValidateAsync("Иван", email,
                "+7 (999) 000-00-00", "123456", "Глава семьи", "");

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Некорректный формат почты", result.Error);
        }

        [TestMethod]
        public async Task Validate_ValidData_ReturnsSuccess()
        {
            // Act
            var result = await _validator.ValidateAsync("Иван", "test@gmail.com", 
                "+7 (999) 000-00-00", "123456", "Глава семьи", "");

            // Assert
            Assert.IsTrue(result.IsValid);
        }
    }
}
