using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VmesteApp.DB.Models;
using VmesteApp.DB.Repository;
using System.Linq;

namespace VmesteApp.Tests
{
    [TestClass]
    public class DataBaseTests
    {
        [TestMethod]
        public void AddEvent_ValidEvent_InsertsIntoDatabase()
        {
            //// Arrange
            //var repo = new EventRepository();
            //var testEvent = new Events
            //{
            //    familyId = 1,
            //    userId = 1,
            //    name = "Тестовое событие",
            //    eventDate = DateTime.Today,

            //    // ТЕПЕРЬ ОШИБКИ НЕ БУДЕТ:
            //    eventTime = DateTime.Now.TimeOfDay,

            //    isPrivate = false
            //};

            //// Act
            //repo.AddEvent(testEvent);

            //// Assert
            //var events = repo.GetFamilyEvents(1, 1);
            //bool exists = events.Any(e => e.name == "Тестовое событие");

            //Assert.IsTrue(exists, "Событие должно быть найдено в базе данных после добавления.");
        }
    }
}
