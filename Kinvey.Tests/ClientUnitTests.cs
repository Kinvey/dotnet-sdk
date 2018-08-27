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
using Newtonsoft.Json.Linq;
using Kinvey;

namespace Kinvey.Tests
{
	[TestClass]
    public class ClientUnitTests : BaseTestClass
	{

		[TestMethod]
		public void TestClientBuilderBasic()
		{
			// Arrange
			const string url = "https://baas.kinvey.com/";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			Client client = builder.Build();

			// Assert
			Assert.IsFalse(client == null);
			Assert.IsFalse(string.IsNullOrEmpty(client.BaseUrl));
			Assert.IsTrue(string.Equals(client.BaseUrl, url));
		}

        [TestMethod]
        public void TestDotnetClientBuilder()
        {
            // Arrange
            var builder = new DotnetClientBuilder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            var client = builder.Build();

            // Assert
            Assert.IsTrue(Constants.DevicePlatform.NET == client.DevicePlatform);
        }

        [TestMethod]
		public void TestClientBuilderBasicBad()
		{
			// Arrange

			// Act
			// Assert
            Assert.ThrowsException<NullReferenceException>(delegate ()
			{
				DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);
			});
		}

        [TestMethod]
		public void TestClientBuilderSetValues()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.setFilePath("")
				.setLogger(delegate (string msg) { Console.WriteLine(msg); });

			// Assert
			Client client = builder.Build();

			Assert.IsFalse(client == null);
            Assert.IsFalse(string.IsNullOrEmpty(client.BaseUrl));
            Assert.IsFalse(client.Store == null);
            Assert.IsFalse(client.logger == null);
            Assert.IsFalse(string.IsNullOrEmpty(client.MICHostName));
		}

        [TestMethod]
		public async Task TestClientBuilderSetOrgID()
		{
			// Arrange
			const string TEST_ORG = "testOrg";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.SetSSOGroupKey(TEST_ORG);
			Client c = builder.Build();

			// Assert
            Assert.IsTrue(c.SSOGroupKey.Equals(TEST_ORG));
		}

        [TestMethod]
		public void TestClientBuilderDoNotSetOrgID()
		{
			// Arrange
			const string TEST_ORG = "testOrg";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			Client c = builder.Build();

			// Assert
            Assert.IsFalse(c.SSOGroupKey.Equals(TEST_ORG));
            Assert.IsTrue(c.SSOGroupKey.Equals(TestSetup.app_key));
		}

        [TestMethod]
		public void ClientBuilderSetBaseURL()
		{
			// Arrange
			const string url = "https://www.test.com/";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.setBaseURL(url);

			// Assert
            Assert.IsFalse(string.IsNullOrEmpty(builder.BaseUrl));
            Assert.IsTrue(string.Equals(builder.BaseUrl, url));
		}

        [TestMethod]
        public void ClientBuilderSetInstanceID()
        {
            // Arrange
            const string instanceID = "testInstanceID";
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            builder.SetInstanceID(instanceID);
            var client = builder.Build();

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(builder.BaseUrl));
            Assert.IsFalse(string.Equals(builder.BaseUrl, AbstractClient.DefaultBaseUrl));
            Assert.IsTrue(string.Equals(builder.BaseUrl, "https://testInstanceID-baas.kinvey.com/"));

            Assert.IsFalse(string.Equals(client.MICHostName, "https://auth.kinvey.com/"));
            Assert.IsTrue(string.Equals(client.MICHostName, "https://testInstanceID-auth.kinvey.com/"));
        }

        [TestMethod]
		public void ClientBuilderSetBaseURLBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			// Assert
            Assert.ThrowsException<KinveyException>(delegate ()
			{
				builder.setBaseURL("www.test.com");
			});
		}

        [TestMethod]
		public async Task TestClientPingAsync()
		{
			// Arrange
			Client.Builder builder = ClientBuilder;
            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }
			Client client = builder.Build();
            if (MockData)
            {
                MockResponses(1, client);
            }

			// Act
			PingResponse pr = await client.PingAsync();

			// Assert
			Assert.IsNotNull(pr.kinvey);
            Assert.IsFalse(pr.kinvey == string.Empty);
			Assert.IsTrue(pr.kinvey.StartsWith("hello"));
			Assert.IsNotNull(pr.version);
            Assert.IsFalse(pr.version == string.Empty);
		}

        [TestMethod]
		public async Task TestClientPingAsyncBad()
		{
			// Arrange
			Client.Builder builder = ClientBuilderFake;
            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }
			Client client = builder.Build();
            if (MockData)
            {
                MockResponses(1, client);
            }

			// Act
            Exception e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
			{
				PingResponse pr = await client.PingAsync();
			});

			// Assert
			Assert.IsTrue(e.GetType() == typeof(KinveyException));
			KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
		}

        [TestMethod]
        public void ClientCheckDefaultMICAPIVersion()
        {
            // Arrange
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            Client testClient = builder.Build();

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(testClient.MICApiVersion));
            Assert.IsTrue(string.Equals(testClient.MICApiVersion, Constants.STR_MIC_DEFAULT_VERSION));
        }

        [TestMethod]
        public void ClientSetMICAPIVersion()
        {
            // Arrange
            string testMICVersion = "v4";
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            Client testClient = builder.Build();
            testClient.MICApiVersion = testMICVersion;

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(testClient.MICApiVersion));
            Assert.IsFalse(string.Equals(testClient.MICApiVersion, Constants.STR_MIC_DEFAULT_VERSION));
            Assert.IsTrue(string.Equals(testClient.MICApiVersion, testMICVersion));
        }
	}
}

