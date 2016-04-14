using System;
using SQLite.Net.Platform.XamarinIOS;
using NUnit.Framework;
using KinveyXamarin;
using KinveyXamariniOS;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestClient
	{
//		private Client kinveyClient;
		private const string user = "testuser";
		private const string pass = "testpass";
		private const string app_key = "abcdefg";
		private const string app_secret = "0123456789abcdef";

		[SetUp]
		public void Setup ()
		{
		}


		[TearDown]
		public void Tear ()
		{
		}


		[Test]
		public void TestClientBuilderBasic()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_key, app_secret);

			// Act
			Client client = builder.build();

			// Assert
			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
			Assert.True(string.Equals(client.BaseUrl, "https://baas.kinvey.com/"));
		}

		[Test]
//		[Ignore("another time")]
		public void TestClientBuilderSetValues()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_key, app_secret);

			// Act
			builder
//				.setCredentialStore(new SQLiteIOSCredentialStore("",""))
				.setFilePath("")
				.setLogger(delegate(string msg) { Console.WriteLine(msg); })
				.setOfflinePlatform(new SQLitePlatformIOS());
//				.build();

			// Assert

			Client client = builder.build();

			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
//			Assert.True(string.Equals(client.BaseUrl, "www.test.com"));
			Assert.False(client.Store == null);
		}

		[Test]
		public void ClientBuilderSetBaseURLBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_key, app_secret);

			// Act
			//builder.setBaseURL("www.test.com");

			// Assert
			Assert.Catch(delegate() {builder.setBaseURL("www.test.com");});
		}

		[Test]
		public void ClientBuilderSetBaseURLGood()
		{
			// Arrange
			const string url = "https://www.test.com/";
			Client.Builder builder = new Client.Builder(app_key, app_secret);

			// Act
			builder.setBaseURL(url);

			// Assert
			Assert.False(string.IsNullOrEmpty(builder.BaseUrl));
			Assert.True(string.Equals(builder.BaseUrl, url));
		}
	}
}
