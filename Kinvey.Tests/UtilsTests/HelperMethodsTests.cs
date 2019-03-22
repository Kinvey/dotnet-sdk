using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Kinvey.Tests
{
    [TestClass]
    public class HelperMethodsTests
    {
        [TestMethod]
        public void TestIsDateMoreRecentFalse()
        {
            //Arrange
            var date = DateTime.Now.ToString();

            //Act
            var result = HelperMethods.IsDateMoreRecent(date, date);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestIsDateMoreRecentTrue()
        {
            //Arrange
            var currentDate = DateTime.Now;

            var checkDate = currentDate.AddDays(1).ToString();
            var origDate = currentDate.ToString();

            //Act
            var result = HelperMethods.IsDateMoreRecent(checkDate, origDate);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestIsDateMoreRecentDefaultFalse()
        {
            //Arrange
            var currentDate = DateTime.Now;

            var checkDate = currentDate.ToString();
            var origDate = currentDate.AddDays(1).ToString();

            //Act
            var result = HelperMethods.IsDateMoreRecent(checkDate, origDate);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
