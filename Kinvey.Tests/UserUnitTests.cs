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
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Kinvey;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;

namespace Kinvey.Tests
{
	[TestClass]
	public class UserUnitTests
    {
        [TestMethod]
        public async Task TestLoginWithMIC()
        {
            // Arrange
            const string authUrl = "https://auth.kinvey.com/oauth/token";
            const string loginUrl = "https://baas.kinvey.com/user/kid_S1GhjUlh7/login";

            var tokenResponse = new HttpResponseMessage();
            var tokenResponseContent = new JObject
            {
                { "access_token", "43468970c7ad26788c87dd7bc3a522daf4e410ee3" },
                { "token_type", "Bearer"},
                { "expires_in", "3599" },
                { "refresh_token", "a093d9b80d4b962345e432c091c7510737b7ca94" }
            };
            tokenResponse.Content = new StringContent(JsonConvert.SerializeObject(tokenResponseContent));
            tokenResponse.StatusCode = System.Net.HttpStatusCode.OK;
            tokenResponse.RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(authUrl)
            };

            var userResponse = new HttpResponseMessage();
            var userResponseContent = JObject.Parse("{\"_id\":\"5be94ecc259d6343d81572ab\",\"_socialIdentity\":{\"kinveyAuth\":{\"id\":\"test\",\"access_token\":\"188ffd5f5188ec2e092925c9981d5acb8bcf6de5\"}},\"username\":\"4b3f5b58-79c8-48e5-abc2-7a9a13f0f33a\",\"_kmd\":{\"lmt\":\"2018-11-12T09:58:36.752Z\",\"ect\":\"2018-11-12T09:58:36.752Z\",\"authtoken\":\"154372da-4fe6-8178-b56d-7ff17c453c6b.w4G7JSPzR/qFmXn1cAtGADts/H7TmGn7yUe0FUe67g4=\"},\"_acl\":{\"creator\":\"5be94ect259d6243d81572ab\"}}");
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(userResponseContent));
            userResponse.StatusCode = System.Net.HttpStatusCode.OK;
            userResponse.RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(loginUrl)
            };

            var moqHttpClientHandler = new Mock<HttpClientHandler>();
            moqHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync((HttpRequestMessage requestMessage, CancellationToken cancellationToken) => requestMessage.RequestUri.AbsoluteUri == authUrl ? tokenResponse : userResponse).Verifiable();

            var credential = new Credential();
            Credential nullCredential = null;

            var credentialStoreMock = new Mock<ICredentialStore>();
            credentialStoreMock.Setup(method => method.Load(It.IsAny<string>(), It.IsAny<string>())).Returns(credential);
            credentialStoreMock.Setup(method => method.Store(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credential>()));
            credentialStoreMock.Setup(method => method.GetStoredCredential(It.IsAny<string>())).Returns(nullCredential);

            var clientBuilder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret).SetRestClient(new HttpClient(moqHttpClientHandler.Object)).setCredentialStore(credentialStoreMock.Object);

            // Act
            Exception exception = null;
            try
            {
                var client = clientBuilder.Build();
                await User.LoginWithMIC("test", "test", "myRedirectURI://", null, client);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);

        }

        [TestMethod]
		public async Task TestMICLoginAutomatedAuthFlowBad()
		{
			// Arrange
			var moqRestClient = new Mock<HttpClientHandler>();
			var moqResponse = new HttpResponseMessage();

            JObject moqResponseContent = new JObject
            {
                { "error", "MOCK RESPONSE ERROR" },
                { "description", "Mock Gaetway Timeout error" },
                { "debug", "Mock debug" }
            };
            moqResponse.Content = new StringContent(JsonConvert.SerializeObject(moqResponseContent));

			moqResponse.StatusCode = System.Net.HttpStatusCode.GatewayTimeout; // Status Code - 504

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
                .SetRestClient(new HttpClient(moqRestClient.Object));

			Client c = cb.Build();
			c.MICApiVersion = "v2";

			string username = "testuser";
			string password = "testpass";
			string redirectURI = "kinveyAuthDemo://";

			// Act
			// Assert
            Exception er = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await User.LoginWithAuthorizationCodeAPIAsync(username, password, redirectURI, c);
			});

            Assert.IsNotNull(er);
			KinveyException ke = er as KinveyException;
			Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
			Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);
			Assert.AreEqual(504, ke.StatusCode); // HttpStatusCode.GatewayTimeout
		}

        [TestMethod]
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
			Assert.IsTrue(clientId == expectedClientId);
            Assert.IsTrue(shouldBeDefaultClientId == appKey);
		}

        [TestMethod]
		public async Task TestMICRenderURLScopeID()
		{
			// Arrange
			var builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);
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
            Assert.IsTrue(signal);
            Assert.IsFalse(urlToTestForScopeID.Equals(string.Empty));
            Assert.IsTrue(urlToTestForScopeID.Contains("scope=openid"));
		}
	}
}
