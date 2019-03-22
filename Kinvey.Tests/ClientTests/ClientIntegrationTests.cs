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
using Kinvey;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Kinvey.Tests
{
	[TestClass]
    public class ClientIntegrationTests: BaseTestClass
	{

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
		public void TestClientBuilderBasicBad()
		{
			// Arrange

			// Act
			// Assert
            Assert.ThrowsException<KinveyException>(delegate ()
			{
				DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);
			});
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
        public void TestClientBuilderSetOrgID()
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
		public async Task TestClientPingAsync()
		{
            Client.Builder builder = ClientBuilder;

			// Arrange
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
            Assert.IsTrue(pr.kinvey.StartsWith("hello", StringComparison.Ordinal));
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
                MockResponses(1);
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
		public async Task TestCustomEndpoint()
		{
            // Arrange
            Client.Builder builder = ClientBuilder.setFilePath(TestSetup.db_dir);

            if (MockData) builder.setBaseURL("http://localhost:8080");

            builder.Build();

            if (MockData) MockResponses(2);

			if (!Client.SharedClient.IsUserLoggedIn())
			{
				await User.LoginAsync(TestSetup.user, TestSetup.pass);
			}

            // Act
            JObject obj = new JObject
            {
                { "input", 1 }
            };

            CustomEndpoint<JObject, ToDo[]> ce = Client.SharedClient.CustomEndpoint<JObject, ToDo[]>();
			var result = await ce.ExecuteCustomEndpoint("test", obj);
			string outputstr = result[1].DueDate;
			int output = int.Parse(outputstr);

			// Assert
			Assert.AreEqual(3, output);
			Assert.AreEqual(2, result.Length);

			// Teardown
			Client.SharedClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestCustomEndpointBad()
		{
            // Arrange
            Client.Builder builder = ClientBuilder
                .SetFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }

			builder.Build();

            if (MockData)
            {
                MockResponses(2);
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass);

            // Act
            JObject obj = new JObject
            {
                { "input", 1 }
            };

            CustomEndpoint<JObject, ToDo[]> ce = Client.SharedClient.CustomEndpoint<JObject, ToDo[]>();
            Exception e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
			{
				await ce.ExecuteCustomEndpoint("test_bad", obj);
			});

			// Teardown
			Client.SharedClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(e);
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
			KinveyException ke = e as KinveyException;
			Assert.AreEqual(404, ke.StatusCode);
		}
	}
}
