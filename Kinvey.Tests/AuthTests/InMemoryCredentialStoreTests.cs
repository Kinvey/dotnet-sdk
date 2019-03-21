using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinvey.Tests
{
    [TestClass]
    public class InMemoryCredentialStoreTests
    {
        private string userId;
        private string ssoGroupKey;
        private Credential credential;
        private InMemoryCredentialStore inMemoryCredentialStore;

        [TestInitialize]
        public void Init()
        {
            userId = Guid.NewGuid().ToString();
            ssoGroupKey = Guid.NewGuid().ToString();
            credential = new Credential();
            inMemoryCredentialStore = new InMemoryCredentialStore();
        }

        [TestCleanup]
        public void Tear()
        {
            inMemoryCredentialStore.Dispose();
        }

        [TestMethod]
        public void TestStoreLoad()
        {
            //Act
            inMemoryCredentialStore.Store(userId, ssoGroupKey, credential);
            var existingCredential = inMemoryCredentialStore.Load(userId, ssoGroupKey);

            //Assert
            Assert.IsNotNull(existingCredential);
        }

        [TestMethod]
        public void TestDelete()
        {
            //Arrange
            inMemoryCredentialStore.Store(userId, ssoGroupKey, credential);

            //Act
            inMemoryCredentialStore.Delete(userId, ssoGroupKey);
            var existingCredential = inMemoryCredentialStore.Load(userId, ssoGroupKey);

            //Assert
            Assert.IsNull(existingCredential);
        }

        [TestMethod]
        public void TestGetStoredCredential()
        {
            //Arrange
            inMemoryCredentialStore.Store(userId, ssoGroupKey, credential);

            //Act
            var existingCredential = inMemoryCredentialStore.GetStoredCredential(ssoGroupKey);

            //Assert
            Assert.IsNotNull(existingCredential);
        }
    }
}
