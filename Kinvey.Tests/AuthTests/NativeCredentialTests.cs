using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class NativeCredentialTests
    {
        private const string userName = "TestUser";
        private const string serializedNativeCredential = "__userID__=TestUser&TestKey=TestValue";
        private const string key = "TestKey";
        private const string value = "TestValue";
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>
        {
            { key, value }
        };

        [TestMethod]
        public void TestSerialize()
        {
            //Arrange

            var nativeCredential = new NativeCredential(userName, properties);

            // Act
            var result = nativeCredential.Serialize();

            //Assert
            Assert.AreEqual(serializedNativeCredential, result);
        }

        [TestMethod]
        public void TestDeserialize()
        {
            //Arrange
            var nativeCredential = new NativeCredential();

            // Act
            var result = NativeCredential.Deserialize(serializedNativeCredential);

            //Assert
            Assert.AreEqual(userName, result.UserID);
            Assert.AreEqual(1, result.Properties.Count);
            Assert.AreEqual(properties[key], result.Properties[key]);
        }
    }
}
