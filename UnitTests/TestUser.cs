using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using KinveyXamarin;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestUser
	{
		private Client kinveyClient;

		private const string newuser = "newuser1";
		private const string newpass = "newpass1";

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(TestSetup.app_key, TestSetup.app_secret).build();
		}

		[TearDown]
		public void Tear ()
		{
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public void TestUserProperties()
		{
			// Arrange

			// Act

			// Assert
		}

		#region Login/Logout Tests

		[Test]
		public async Task TestLoginAsync()
		{
			// Arrange

			// Act
			await kinveyClient.CurrentUser.LoginAsync();

			// Assert
			Assert.NotNull(kinveyClient.CurrentUser);
			Assert.True(kinveyClient.CurrentUser.isUserLoggedIn());

			// Teardown
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestLoginAsyncBad()
		{
			// Arrange
			Client fakeClient = new Client.Builder(TestSetup.app_key_fake, TestSetup.app_secret_fake).build();

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await fakeClient.CurrentUser.LoginAsync();
			});

			Assert.Catch(async delegate() {
				await fakeClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);
			});
		}

		[Test]
		public async Task TestLoginUserPassAsync()
		{
			// Arrange

			// Act
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Assert
			Assert.NotNull(kinveyClient.CurrentUser);
			Assert.True(kinveyClient.CurrentUser.isUserLoggedIn());

			// Teardown
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginUserPassAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginFacebookAsync()
		{
			// Arrange
			string facebookAccessToken = "";

			// Act
			User fbUser = await kinveyClient.CurrentUser.LoginFacebookAsync(facebookAccessToken);

			// Assert
			Assert.IsNotNull(fbUser);
			Assert.IsNotNull(fbUser.Attributes["_socialIdentity"]);
			JToken socID = fbUser.Attributes["_socialIdentity"];
			Assert.IsNotNull(socID["facebook"]);
			Assert.True(socID["facebook"].HasValues);
		}

		[Test]
		public async Task TestLoginFacebookAsyncBad()
		{
			// Arrange
			string facebookAccessTokenBad = "blahblahblah";

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await kinveyClient.CurrentUser.LoginFacebookAsync(facebookAccessTokenBad);
			});
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginGoogleAsync()
		{
			// Arrange
			string googleAccessToken = "";

			// Act
			User googleUser = await kinveyClient.CurrentUser.LoginGoogleAsync(googleAccessToken);

			// Assert
			Assert.IsNotNull(googleUser);
			Assert.IsNotNull(googleUser.Attributes["_socialIdentity"]);
			JToken socID = googleUser.Attributes["_socialIdentity"];
			Assert.IsNotNull(socID["google"]);
			Assert.True(socID["google"].HasValues);
		}

		[Test]
		public async Task TestLoginGoogleAsyncBad()
		{
			// Arrange
			string googleAccessTokenBad = "blahblahblah";

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await kinveyClient.CurrentUser.LoginGoogleAsync(googleAccessTokenBad);
			});
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginTwitterAsync()
		{
			// Arrange
			string accessTokenKey = "";
			string accessTokenSecret = "";
			string consumerKey = "";
			string consumerKeySecret = "";

			// Act
			User twitterUser = await kinveyClient.CurrentUser.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret);

			// Assert
			Assert.IsNotNull(twitterUser);
			Assert.IsNotNull(twitterUser.Attributes["_socialIdentity"]);
			JToken socID = twitterUser.Attributes["_socialIdentity"];
			Assert.IsNotNull(socID["twitter"]);
			Assert.True(socID["twitter"].HasValues);
		}

		[Test]
		public async Task TestLoginTwitterAsyncBad()
		{
			// Arrange
			string accessTokenKey = "twitterAccessTokenBad";
			string accessTokenSecret = "twitterAccessTokenSecretBad";
			string consumerKey = "twitterConsumerKeyBad";
			string consumerKeySecret = "twitterConsumerKeySecretBad";

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await kinveyClient.CurrentUser.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret);
			});
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginLinkedInAsync()
		{
			// Arrange
			string accessTokenKey = "";
			string accessTokenSecret = "";
			string consumerKey = "";
			string consumerKeySecret = "";

			// Act
			User linkedinUser = await kinveyClient.CurrentUser.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret);

			// Assert
			Assert.IsNotNull(linkedinUser);
			Assert.IsNotNull(linkedinUser.Attributes["_socialIdentity"]);
			JToken socID = linkedinUser.Attributes["_socialIdentity"];
			Assert.IsNotNull(socID["linkedin"]);
			Assert.True(socID["linkedin"].HasValues);
		}

		[Test]
		public async Task TestLoginLinkedInAsyncBad()
		{
			// Arrange
			string accessTokenKey = "twitterAccessTokenBad";
			string accessTokenSecret = "twitterAccessTokenSecretBad";
			string consumerKey = "twitterConsumerKeyBad";
			string consumerKeySecret = "twitterConsumerKeySecretBad";

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await kinveyClient.CurrentUser.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret);
			});
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginSalesforceAsync()
		{
			// Arrange
			string access = "";
			string reauth = "";
			string clientID = "";
			string ID = "";

			// Act
			User salesforceUser = await kinveyClient.CurrentUser.LoginSalesforceAsync(access, reauth, clientID, ID);

			// Assert
			Assert.IsNotNull(salesforceUser);
			Assert.IsNotNull(salesforceUser.Attributes["_socialIdentity"]);
			JToken socID = salesforceUser.Attributes["_socialIdentity"];
			Assert.IsNotNull(socID["salesforce"]);
			Assert.True(socID["salesforce"].HasValues);
		}

		[Test]
		public async Task TestLoginSalesforceAsyncBad()
		{
			// Arrange
			string access = "";
			string reauth = "";
			string clientID = "";
			string ID = "";

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await kinveyClient.CurrentUser.LoginSalesforceAsync(access, reauth, clientID, ID);
			});
		}

		// MIC LOGIN TESTS
		//
		[Test]
		public async Task TestMIC_LoginWithAuthorizationCodeLoginPage()
		{
			// Arrange
			string redirectURI = "http://test.redirect";

			// Act
			string renderURL = await kinveyClient.CurrentUser.LoginWithAuthorizationCodeLoginPage(redirectURI);

			// Assert
			Assert.IsNotNullOrEmpty(renderURL);
			Assert.True(renderURL.StartsWith(kinveyClient.MICHostName + "oauth/auth?client_id"));
		}

		[Test]
		[Ignore("Placeholder - Need configured backend to run test")]
		public async Task TestMIC_LoginWithAuthorizationCodeAPI()
		{
			// Arrange
			string username = "testuser";
			string password = "testpass";
			string redirectURI = "kinveyAuthDemo://";
			string saml_app_key = "kid_ZkPDb_34T";
			string saml_app_secret = "c3752d5079f34353ab89d07229efaf63";
			Client localClient = new Client.Builder(saml_app_key, saml_app_secret).build();
			localClient.MICApiVersion = "v2";

			// Act
			await localClient.CurrentUser.LoginWithAuthorizationCodeAPI(username, password, redirectURI);

			// Assert
			Assert.True(localClient.CurrentUser.isUserLoggedIn());

			// Teardown
			localClient.CurrentUser.Logout();
		}

		#endregion

		#region CRUD Tests

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCreateUserAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			string email = "newuser@test.com";
			Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
			customFields.Add("email", email);

			// Act
			User newUser = await kinveyClient.CurrentUser.CreateAsync("newuser1", "newpass1", customFields);

			// Assert
			Assert.NotNull(newUser);
			Assert.NotNull(newUser.Attributes);
//			Assert.AreSame(newUser.Attributes["email"], email);

			// Teardown
//			await kinveyClient.CurrentUser.DeleteAsync(newUser.Id, true);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCreateUserAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestFindUserAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange

			// Act
			User me = await kinveyClient.CurrentUser.RetrieveAsync();

			// Assert
			Assert.NotNull(me);
			Assert.True(string.Equals(kinveyClient.CurrentUser.Id, me.Id)); 

			// Teardown
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestFindUserAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestLookupUsersAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			UserDiscovery criteria = new UserDiscovery();
			criteria.FirstName = "George";

			// Act
			User[] users = await kinveyClient.CurrentUser.LookupAsync(criteria);

			// Assert
			Assert.NotNull(users);
			Assert.AreEqual(3, users.Length);

			// Teardown
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLookupUsersAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestUpdateUserAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestUpdateUserAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserSoftAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserSoftAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async void TestDeleteUserHardAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async void TestDeleteUserHardAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		#endregion
	}
}
