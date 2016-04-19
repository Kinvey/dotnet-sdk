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

		private const string user = "testuser";
		private const string pass = "testpass";

		private const string app_id_fake = "abcdefg";
		private const string app_secret_fake = "0123456789abcdef";

		private const string app_id = "kid_Zy0JOYPKkZ";
		private const string app_secret = "d83de70e64d540e49acd6cfce31415df";

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(app_id, app_secret).build();
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

		// LOGIN/LOGOUT TESTS
		//

		[Test]
		public async Task TestLoginAsyncBad()
		{
			// Arrange
			Client fakeClient = new Client.Builder(app_id_fake, app_secret_fake).build();

			// Act
			// Assert
			Assert.Catch(async delegate() {
				await fakeClient.CurrentUser.LoginAsync();
			});

			Assert.Catch(async delegate() {
				await fakeClient.CurrentUser.LoginAsync(user, pass);
			});
		}

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
		public async Task TestLoginUserPassAsync()
		{
			// Arrange

			// Act
			await kinveyClient.CurrentUser.LoginAsync(user, pass);

			// Assert
			Assert.NotNull(kinveyClient.CurrentUser);
			Assert.True(kinveyClient.CurrentUser.isUserLoggedIn());

			// Teardown
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginFacebookAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginFacebookAsyncBad()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginGoogleAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginGoogleAsyncBad()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginTwitterAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginTwitterAsyncBad()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginSalesforceAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLoginSalesforceAsyncBad()
		{
		}

		// CREATE TESTS
		//

		[Test]
		public async Task TestCreateUserAsync()
		{
			// Arrange
			string email = "newuser@test.com";
			Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
			customFields.Add("email", email);

			// Act
			User newUser = await kinveyClient.CurrentUser.CreateAsync("newuser1", "newpass1", customFields);

			// Assert
			Assert.NotNull(newUser);
//			Assert.NotNull(newUser.Attributes);
//			Assert.AreSame(newUser.Attributes["email"], email);

			// Teardown
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

		// READ TESTS
		//
		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestFindUserAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestFindUserAsyncBad()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLookupUsersAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLookupUsersAsyncBad()
		{
		}

		// UPDATE TESTS
		//
		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestUpdateUserAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestUpdateUserAsyncBad()
		{
		}

		// DELETE TESTS
		//

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserSoftAsync()
		{
//			// Arrange
//			string userID = "4567";
//
//			// Act
//			User.DeleteRequest deleteRequest = await kinveyClient.User().DeleteAsync(userID, false);
//
//			// Assert
//			Assert.True(deleteRequest.RequestMethod == "DELETE");
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserSoftAsyncBad()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async void TestDeleteUserHardAsync()
		{
//			string userID;
//
//			User.DeleteRequest deleteRequest = await kinveyClient.User().DeleteAsync(userID, true);
//
//			// Assert
//			Assert.True(deleteRequest.RequestMethod == "DELETE");
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async void TestDeleteUserHardAsyncBad()
		{
		}
	}
}
