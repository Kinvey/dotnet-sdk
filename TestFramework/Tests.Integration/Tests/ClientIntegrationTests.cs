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
using Kinvey;
using Newtonsoft.Json.Linq;

namespace TestFramework
{
	[TestFixture]
	public class ClientIntegrationTests
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
		public void TestClientBuilderSetOrgID()
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
		public async Task TestCustomEndpoint()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric());

			builder.Build();

			if (!Client.SharedClient.IsUserLoggedIn())
			{
				await User.LoginAsync(TestSetup.user, TestSetup.pass);
			}

			// Act
			JObject obj = new JObject();
			obj.Add("input", 1);

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

		[Test]
		public async Task TestCustomEndpointBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric());

			builder.Build();

			if (!Client.SharedClient.IsUserLoggedIn())
			{
				await User.LoginAsync(TestSetup.user, TestSetup.pass);
			}

			// Act
			JObject obj = new JObject();
			obj.Add("input", 1);

			CustomEndpoint<JObject, ToDo[]> ce = Client.SharedClient.CustomEndpoint<JObject, ToDo[]>();
			Exception e = Assert.CatchAsync(async delegate
			{
				await ce.ExecuteCustomEndpoint("test_bad", obj);
			});

			// Teardown
			Client.SharedClient.ActiveUser.Logout();

			// Assert
			Assert.NotNull(e);
			Assert.True(e.GetType() == typeof(KinveyException));
			KinveyException ke = e as KinveyException;
			Assert.AreEqual(404, ke.StatusCode);
		}
	}
}
