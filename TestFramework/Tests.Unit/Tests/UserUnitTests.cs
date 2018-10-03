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
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Kinvey;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace TestFramework
{
	[TestFixture]
	public class UserUnitTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Tear()
		{
		}

		[Ignore("Placeholder - No unit test yet")]
		[Test]
		public async Task TestUserBasic()
		{
            // Arrange
            Mock<HttpClient> moqRC = new Mock<HttpClient>();
            var resp = new HttpResponseMessage
            {
                Content = new StringContent("MOCK RESPONSE")
            };
            moqRC
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(resp)
                .Verifiable();

			// Act

			// Assert
		}

		[Test]
		public async Task TestMICLoginAutomatedAuthFlowBad()
		{
			// Arrange
            var moqRestClient = new Mock<HttpClientHandler>();
            var moqResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.GatewayTimeout, // Status Code - 504
                Content = new StringContent(JsonConvert.SerializeObject(new JObject
                {
                    { "error", "MOCK RESPONSE ERROR" },
                    { "description", "Mock Gaetway Timeout error" },
                    { "debug", "Mock debug" }
                })),
            };

            moqRestClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(moqResponse)
                .Verifiable();

			Client.Builder cb = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
                .SetRestClient(new HttpClient(moqRestClient.Object));

			Client c = cb.Build();
			c.MICApiVersion = "v2";

			string username = "testuser";
			string password = "testpass";
			string redirectURI = "kinveyAuthDemo://";

			// Act
			// Assert
			Exception er = Assert.CatchAsync(async delegate ()
			{
				await User.LoginWithAuthorizationCodeAPIAsync(username, password, redirectURI, c);
			});

			Assert.NotNull(er);
			KinveyException ke = er as KinveyException;
			Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
			Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);
			Assert.AreEqual(504, ke.StatusCode); // HttpStatusCode.GatewayTimeout
		}

		[Test]
		public async Task TestMICValidateAuthServiceID()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);
			Client client = builder.Build();
			string appKey = ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey;
			string micID = "12345";
			string expectedClientId = TestSetup.app_key + "." + micID;

			// Act

			// Test AuthServiceID after setting a clientId
			var requestWithClientID = User.GetMICToken(client, "fake_code", appKey + Constants.CHAR_PERIOD + micID);
			string clientId = ((KinveyClientRequestInitializer)client.RequestInitializer).AuthServiceID;

			// Test to verify that initializing a request other than `/oauth/token` will
			// reset the AuthServiceID back to the default, which is AppKey.
			var req = User.BuildMICTempURLRequest(client, null);
			string shouldBeDefaultClientId = ((KinveyClientRequestInitializer)client.RequestInitializer).AuthServiceID;

			// Assert
			Assert.True(clientId == expectedClientId);
			Assert.True(shouldBeDefaultClientId == appKey);
		}

		[Test]
		public async Task TestMICRenderURLScopeID()
		{
			// Arrange
			var builder = new DotnetClientBuilder(TestSetup.app_key, TestSetup.app_secret);
			var client = builder.Build();
			var autoEvent = new System.Threading.AutoResetEvent(false);
			string urlToTestForScopeID = String.Empty;

			var micDelegate = new KinveyMICDelegate<User>()
			{
				onError = (user) => { },
				onSuccess = (error) => { },
				onReadyToRender = (url) => {
					urlToTestForScopeID = url;
					autoEvent.Set();
				}
			};

			// Act
			User.LoginWithMIC("mytestredirectURI", micDelegate);

			bool signal = autoEvent.WaitOne(5000);

			// Assert
			Assert.True(signal);
			Assert.False(urlToTestForScopeID.Equals(string.Empty));
			Assert.That(urlToTestForScopeID.Contains("scope=openid"));
		}

        [Test]
        public async Task TestMICOnRedirectErrorParsing()
        {
            // Arrange
            var error = "12345";
            var errorDescription = "test error description";
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);
            Client client = builder.Build();
            var loginRequest = new User.LoginToTempURLRequest(client, string.Empty, new System.Collections.Generic.Dictionary<string, string>(){{ "client_id", "none" }}, null);
            string redirectUri = $"myredirecturi/error={error}&error_description={errorDescription}"; //error=<error code>&error_description=<error description text>

            // Act
            // Assert
            Exception e = Assert.CatchAsync(async delegate {
                await loginRequest.onRedirectAsync(redirectUri);
            });

            Assert.True(e.GetType() == typeof(KinveyException));
            var ke = e as KinveyException;
            Assert.AreEqual(ke.ErrorCategory, EnumErrorCategory.ERROR_USER);
            Assert.AreEqual(ke.ErrorCode, EnumErrorCode.ERROR_MIC_REDIRECT_ERROR);
            Assert.True(ke.Error.EndsWith(error));
            Assert.AreEqual(ke.Description, errorDescription);
        }
    }
}
