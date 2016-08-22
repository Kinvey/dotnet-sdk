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

		private const string collectionName = "ToDos";

		[SetUp]
		public void Setup()
		{
			kinveyClient = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
				.build();
		}

		[TearDown]
		public void Tear()
		{
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);
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
			User u = await User.LoginAsync(kinveyClient);

			// Assert
			Assert.NotNull(kinveyClient.ActiveUser);
			Assert.True(u.IsActive());

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSharedClientLoginAsync()
		{
			// Arrange

			// Act
			User u = await User.LoginAsync(Client.SharedClient);

			// Assert
			Assert.NotNull(Client.SharedClient.ActiveUser);
			Assert.True(u.IsActive());

			// Teardown
			Client.SharedClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestLoginAsyncBad()
		{
			// Arrange
			Client fakeClient = new Client.Builder(TestSetup.app_key_fake, TestSetup.app_secret_fake).build();

			// Act
			// Assert
			Assert.CatchAsync(async delegate() {
				await User.LoginAsync(fakeClient);
			});

			Assert.CatchAsync(async delegate() {
				await User.LoginAsync(TestSetup.user, TestSetup.pass, fakeClient);
			});
		}

		[Test]
		public async Task TestLoginUserPassAsync()
		{
			// Arrange

			// Act
			User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Assert
			Assert.NotNull(kinveyClient.ActiveUser);
			Assert.True(u.IsActive());

			// Teardown
			kinveyClient.ActiveUser.Logout();
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
			User fbUser = await User.LoginFacebookAsync(facebookAccessToken, kinveyClient);

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
			Assert.CatchAsync(async delegate() {
				await User.LoginFacebookAsync(facebookAccessTokenBad, kinveyClient);
			});
		}

		[Test]
		[Ignore("Placeholder - Need Access Token To Run Test")]
		public async Task TestLoginGoogleAsync()
		{
			// Arrange
			string googleAccessToken = "";

			// Act
			User googleUser = await User.LoginGoogleAsync(googleAccessToken, kinveyClient);

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
			Assert.CatchAsync(async delegate() {
				await User.LoginGoogleAsync(googleAccessTokenBad, kinveyClient);
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
			User twitterUser = await User.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);

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
			Assert.CatchAsync(async delegate() {
				await User.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);
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
			User linkedinUser = await User.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);

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
			Assert.CatchAsync(async delegate() {
				await User.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);
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
			User salesforceUser = await User.LoginSalesforceAsync(access, reauth, clientID, ID, kinveyClient);

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
			Assert.CatchAsync(async delegate() {
				await User.LoginSalesforceAsync(access, reauth, clientID, ID, kinveyClient);
			});
		}

		// MIC LOGIN TESTS
		//
		[Test]
		public async Task TestMIC_LoginWithAuthorizationCodeLoginPage()
		{
			// Arrange
			string redirectURI = "http://test.redirect";
			User loggedInUser = null;

			// Act
			string renderURL = null;
			User.LoginWithAuthorizationCodeLoginPage(redirectURI, new KinveyMICDelegate<User>{
				onSuccess = (user) => { loggedInUser = user; },
				onError = (e) => { Console.WriteLine("TEST MIC ERROR"); },
				onReadyToRender = (url) => { renderURL = url; }
			});

			// Assert
			Assert.IsNotNull(renderURL);
			Assert.IsNotEmpty(renderURL);
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
			await localClient.ActiveUser.LoginWithAuthorizationCodeAPIAsync(username, password, redirectURI);

			// Assert
			Assert.NotNull(localClient.ActiveUser);

			// Teardown
			localClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestLogout ()
		{
			// Arrange
			await User.LoginAsync (TestSetup.user, TestSetup.pass, kinveyClient);
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
			ToDo td = new ToDo();
			td.Name = "test";
			await todoStore.SaveAsync(td);

			DataStore<FlashCard> flashCardStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC, kinveyClient);
			FlashCard fc = new FlashCard ();
			fc.Answer = "huh";
			await flashCardStore.SaveAsync (fc);

			// Act
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.Null(kinveyClient.ActiveUser);
			Assert.IsEmpty(kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1,0));
		}

		[Test]
		public async Task TestLogoutWithNoDatabaseTables()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Act
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.Null(kinveyClient.ActiveUser);
			Assert.IsEmpty(kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1, 0));
		}

		[Test]
		public async Task TestLogoutWithDatabaseTablesButNoAPICalls()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);

			// Act
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.Null(kinveyClient.ActiveUser);
			Assert.IsEmpty(kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1, 0));
		}

		#endregion

		#region CRUD Tests

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCreateUserAsync()
		{
			// Arrange
			string email = "newuser@test.com";
			Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
			customFields.Add("email", email);

			// Act
			User newUser = await User.SignupAsync("newuser1", "newpass1", customFields, kinveyClient);

			// Teardown
			//await kinveyClient.ActiveUser.DeleteAsync(newUser.Id, true);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.NotNull(newUser);
			Assert.NotNull(newUser.Attributes);
			Assert.True(String.Compare((newUser.Attributes["email"]).ToString(), email) == 0);
		}

		[Test]
		public async Task TestCreateUserAsyncBad()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string email = "newuser@test.com";
			Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
			customFields.Add("email", email);

			// Act
			Exception er = Assert.CatchAsync(async delegate () {
				await User.SignupAsync("newuser1", "newpass1", customFields, kinveyClient);
			});

			// Assert
			Assert.NotNull(er);
			KinveyException ke = er as KinveyException;
			Assert.AreEqual(EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN, ke.ErrorCode);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestFindUserAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange

			// Act
			User me = await kinveyClient.ActiveUser.RefreshAsync();

			// Assert
			Assert.NotNull(me);
			Assert.True(string.Equals(kinveyClient.ActiveUser.Id, me.Id)); 

			// Teardown
			kinveyClient.ActiveUser.Logout();
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
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			UserDiscovery criteria = new UserDiscovery();
			criteria.FirstName = "George";

			// Act
			User[] users = await kinveyClient.ActiveUser.LookupAsync(criteria);

			// Assert
			Assert.NotNull(users);
			Assert.AreEqual(3, users.Length);

			// Teardown
			kinveyClient.ActiveUser.Logout();
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
		public async Task TestDoesUsernameExist()
		{
			// Arrange
			string username = "testuser";

			// Act
			bool exists = await User.HasUser(username);

			// Assert
			Assert.True(exists);
		}

		[Test]
		public async Task TestDoesUsernameExistBad()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string username = "homer_simpson";

			// Act
			bool exists = await User.HasUser(username);

			// Assert
			Assert.False(exists);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestForgotUsername()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string email = "vinay@kinvey.com";

			// Act
			// Assert
			Assert.DoesNotThrowAsync(async delegate () {
				await User.ForgotUsername(email);
			});

			// Teardown
			kinveyClient.ActiveUser.Logout();
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
		public async Task TestDeleteUserHardAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserHardAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		#endregion

		[Test]
		public async Task TestUserKMDEmailVerification()
		{
			// Setup
			User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			u.Metadata = new KinveyUserMetaData();
			u.Metadata.EmailVerification.Status = "sent";

			// Act
			string status = u.Metadata.EmailVerification.Status;

			// Assert
			Assert.True(String.Equals(status, "sent"));

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestUserKMDPasswordReset()
		{
			// Setup
			User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			u.Metadata = new KinveyUserMetaData();
			u.Metadata.PasswordReset.Status = "InProgress";

			// Act
			string status = u.Metadata.PasswordReset.Status;

			// Assert
			Assert.True(String.Equals(status, "InProgress"));

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}
	}
}
