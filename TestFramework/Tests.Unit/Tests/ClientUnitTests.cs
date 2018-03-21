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
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Kinvey;

namespace TestFramework
{
	[TestFixture]
	public class ClientUnitTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Tear()
		{
		}

		[Test]
		public void TestClientBuilderBasic()
		{
			// Arrange
			const string url = "https://baas.kinvey.com/";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			Client client = builder.Build();

			// Assert
			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
			Assert.True(string.Equals(client.BaseUrl, url));
		}

		[Test]
		public void TestClientBuilderBasicBad()
		{
			// Arrange

			// Act
			// Assert
			Assert.Catch(delegate ()
			{
				DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);
			});
		}

		[Test]
		public void TestClientBuilderSetValues()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.setFilePath("")
				.setLogger(delegate (string msg) { Console.WriteLine(msg); });

			// Assert
			Client client = builder.Build();

			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
			Assert.False(client.Store == null);
			Assert.False(client.logger == null);
			Assert.False(string.IsNullOrEmpty(client.MICHostName));
		}

		[Test]
		public async Task TestClientBuilderSetOrgID()
		{
			// Arrange
			const string TEST_ORG = "testOrg";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.SetSSOGroupKey(TEST_ORG);
			Client c = builder.Build();

			// Assert
			Assert.True(c.SSOGroupKey.Equals(TEST_ORG));
		}

		[Test]
		public void TestClientBuilderDoNotSetOrgID()
		{
			// Arrange
			const string TEST_ORG = "testOrg";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			Client c = builder.Build();

			// Assert
			Assert.False(c.SSOGroupKey.Equals(TEST_ORG));
			Assert.True(c.SSOGroupKey.Equals(TestSetup.app_key));
		}

		[Test]
		public void ClientBuilderSetBaseURL()
		{
			// Arrange
			const string url = "https://www.test.com/";
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			builder.setBaseURL(url);

			// Assert
			Assert.False(string.IsNullOrEmpty(builder.BaseUrl));
			Assert.True(string.Equals(builder.BaseUrl, url));
		}

        [Test]
        public void ClientBuilderSetInstanceID()
        {
            // Arrange
            const string instanceID = "testInstanceID";
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            builder.SetInstanceID(instanceID);
            var client = builder.Build();

            // Assert
            Assert.False(string.IsNullOrEmpty(builder.BaseUrl));
            Assert.False(string.Equals(builder.BaseUrl, AbstractClient.DefaultBaseUrl));
            Assert.True(string.Equals(builder.BaseUrl, "https://testInstanceID-baas.kinvey.com/"));

            Assert.False(string.Equals(client.MICHostName, "https://auth.kinvey.com/"));
            Assert.True(string.Equals(client.MICHostName, "https://testInstanceID-auth.kinvey.com/"));
        }

		[Test]
		public void ClientBuilderSetBaseURLBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			// Assert
			Assert.Catch(delegate ()
			{
				builder.setBaseURL("www.test.com");
			});
		}

		[Test]
		public async Task TestClientPingAsync()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);
			Client client = builder.Build();

			// Act
			PingResponse pr = await client.PingAsync();

			// Assert
			Assert.IsNotNull(pr.kinvey);
			Assert.IsNotEmpty(pr.kinvey);
			Assert.True(pr.kinvey.StartsWith("hello"));
			Assert.IsNotNull(pr.version);
			Assert.IsNotEmpty(pr.version);
		}

		[Test]
		public async Task TestClientPingAsyncBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key_fake, TestSetup.app_secret_fake);
			Client client = builder.Build();

			// Act
			Exception e = Assert.CatchAsync(async delegate
			{
				PingResponse pr = await client.PingAsync();
			});

			// Assert
			Assert.True(e.GetType() == typeof(KinveyException));
			KinveyException ke = e as KinveyException;
			Assert.True(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
			Assert.True(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCustomEndpoint()
		{
			// Arrange

			// Act

			// Assert
		}

        [Test]
        public void ClientCheckDefaultMICAPIVersion()
        {
            // Arrange
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            Client testClient = builder.Build();

            // Assert
            Assert.False(string.IsNullOrEmpty(testClient.MICApiVersion));
            Assert.True(string.Equals(testClient.MICApiVersion, Constants.STR_MIC_DEFAULT_VERSION));
        }

        [Test]
        public void ClientSetMICAPIVersion()
        {
            // Arrange
            string testMICVersion = "v4";
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            // Act
            Client testClient = builder.Build();
            testClient.MICApiVersion = testMICVersion;

            // Assert
            Assert.False(string.IsNullOrEmpty(testClient.MICApiVersion));
            Assert.False(string.Equals(testClient.MICApiVersion, Constants.STR_MIC_DEFAULT_VERSION));
            Assert.True(string.Equals(testClient.MICApiVersion, testMICVersion));
        }
	}
}

