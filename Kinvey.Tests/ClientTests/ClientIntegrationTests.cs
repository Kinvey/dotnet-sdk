// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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
        public void TestClientBuilderWithInvalidRequirements()
        {
            // Arrange
            var builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

            DynamicBuilder.CreateAssemblyWithType();

            // Act
            var exception = Assert.ThrowsException<KinveyException>(delegate
            {
                builder.Build();
            });

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_REQUIREMENT, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_REQUIREMENT_MISSING_GET_SET_ACCESSORS, ke.ErrorCode);
            Assert.AreEqual("There is the incorrect field TestField in the type TestType", ke.Info);

            //Teardown

            /* The type `TestType` was created dynamically and added to the primary domain above.
            This type must be unloaded from the domain to avoid exceptions in other tests as they use the `Client.Builder` class too.
            There is not any explicit possibility to do it in .Net Core like it was in .Net Framework. 
            For example, unloading an assembly from a domain or a whole domain.  
            There is only some implicit possibility.This is the using of GC. In this case the assembly with the `TestType` type will be unloaded from the domain by means of GC like it is no longer accessible.
            After this, all subsequent tests will run correctly. */
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            if (MockData)
            {
                // Arrange
                Client.Builder builder = ClientBuilder.setFilePath(TestSetup.db_dir);
                builder.setBaseURL("http://localhost:8080");
                builder.Build();

                MockResponses(2);

                 await User.LoginAsync(TestSetup.user, TestSetup.pass);

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
            }
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

        [TestMethod]
        public async Task TestSetApiVersionAuthRequests()
        {
            // Arrange
            var notSupportingApiVersion = int.MaxValue.ToString();

            var builder = new Client.Builder(AppKey, AppSecret);
            builder.SetFilePath(TestSetup.db_dir);
           
            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
                builder.setMICHostName("http://localhost:8081");
            }

            var client1 = builder.Build();

            builder.SetApiVersion(notSupportingApiVersion);
            var client2 = builder.Build();

            builder.SetApiVersion(KinveyHeaders.kinveyApiVersion);
            var client3 = builder.Build();

            if (MockData)
            {
                MockResponses(3);
            }
          
            // Act
            var userForClient1 = await User.LoginAsync(client1);

            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await User.LoginAsync(client2);
            });

            var userForClient3 = await User.LoginAsync(client3);
           
            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var kinveyException = exception as KinveyException;
            Assert.IsTrue(kinveyException.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(kinveyException.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);

            Assert.IsNotNull(client1.ActiveUser);
            Assert.IsTrue(userForClient1.Active);

            Assert.IsNotNull(client3.ActiveUser);
            Assert.IsTrue(userForClient3.Active);

            Assert.AreEqual(KinveyHeaders.kinveyApiVersion, client1.ApiVersion);
            Assert.AreEqual(notSupportingApiVersion, client2.ApiVersion);
            Assert.AreEqual(KinveyHeaders.kinveyApiVersion, client3.ApiVersion);
        }

        [TestMethod]
        public async Task TestSetApiVersionKinveyClientRequests()
        {
            // Arrange
            var notSupportingApiVersion = int.MaxValue.ToString();

            var builder = new Client.Builder(AppKey, AppSecret);
            builder.SetFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
                builder.setMICHostName("http://localhost:8081");
            }

            var client1 = builder.Build();

            builder.SetApiVersion(notSupportingApiVersion);
            var client2 = builder.Build();

            builder.SetApiVersion(KinveyHeaders.kinveyApiVersion);
            var client3 = builder.Build();

            if (MockData)
            {
                MockResponses(3);
            }

            // Act
            var pingResponse1 = await client1.PingAsync();

            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await client2.PingAsync();
            });

            var pingResponse3 = await client3.PingAsync();

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var kinveyException = exception as KinveyException;
            Assert.IsTrue(kinveyException.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(kinveyException.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);

            Assert.IsNotNull(pingResponse1.kinvey);
            Assert.IsTrue(pingResponse1.kinvey.StartsWith("hello", StringComparison.Ordinal));
            Assert.IsNotNull(pingResponse1.version);

            Assert.IsNotNull(pingResponse3.kinvey);
            Assert.IsTrue(pingResponse3.kinvey.StartsWith("hello", StringComparison.Ordinal));
            Assert.IsNotNull(pingResponse3.version);

            Assert.AreEqual(KinveyHeaders.kinveyApiVersion, client1.ApiVersion);
            Assert.AreEqual(notSupportingApiVersion, client2.ApiVersion);
            Assert.AreEqual(KinveyHeaders.kinveyApiVersion, client3.ApiVersion);
        }
    }
}
