// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinvey.Tests
{
	[TestClass]
	public class UserUnitTests : BaseTestClass
    {
        private Client kinveyClient;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            //var builder = ClientBuilder.SetFilePath(TestSetup.db_dir);

            Client.Builder builder = ClientBuilder;

            if (MockData)
            {
                builder = builder.setBaseURL("http://localhost:8080").setMICHostName("http://localhost:8080");
                //ClientBuilder.setBaseURL("http://192.168.1.34").setMICHostName("http://192.168.1.34");
            }

            kinveyClient = builder.Build();
        }

        [TestMethod]
        public async Task TestLoginWithMIC()
        {
            // Arrange
            if (MockData)
            {
                MockResponses(2);
            }

            // Act
            Exception exception = null;
            try
            {
                await User.LoginWithMIC("test", "test", null, kinveyClient);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void TestMICValidateAuthServiceID()
        {
            // Arrange
            string appKey = ((KinveyClientRequestInitializer)kinveyClient.RequestInitializer).AppKey;
            string micID = "12345";
            string expectedClientId = TestSetup.app_key + "." + micID;

            // Act
            // Test AuthServiceID after setting a clientId
            var requestWithClientID = User.GetMICToken(kinveyClient, "fake_code", appKey + Constants.CHAR_PERIOD + micID);
            string clientId = ((KinveyClientRequestInitializer)kinveyClient.RequestInitializer).AuthServiceID;

            // Test to verify that initializing a request other than `/oauth/token` will
            // reset the AuthServiceID back to the default, which is AppKey.
            var req = User.BuildMICTempURLRequest(kinveyClient, null);
            string shouldBeDefaultClientId = ((KinveyClientRequestInitializer)kinveyClient.RequestInitializer).AuthServiceID;

            // Assert
            Assert.IsTrue(clientId == expectedClientId);
            Assert.IsTrue(shouldBeDefaultClientId == appKey);
        }

        [TestMethod]
        public void TestMICRenderURLScopeID()
        {
            // Arrange
            var autoEvent = new System.Threading.AutoResetEvent(false);
            string urlToTestForScopeID = String.Empty;

            var micDelegate = new KinveyMICDelegate<User>()
            {
                onError = (user) => { },
                onSuccess = (error) => { },
                onReadyToRender = (url) =>
                {
                    urlToTestForScopeID = url;
                    autoEvent.Set();
                }
            };

            // Act
            User.LoginWithMIC("mytestredirectURI", micDelegate);

            bool signal = autoEvent.WaitOne(5000);

            // Assert
            Assert.IsTrue(signal);
            Assert.IsFalse(urlToTestForScopeID.Equals(string.Empty));
            Assert.IsTrue(urlToTestForScopeID.Contains("scope=openid"));
        }
    }
}
