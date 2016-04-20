using System;
using System.Threading.Tasks;
using NUnit.Framework;
using KinveyXamarin;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestClient
	{
		//		private Client kinveyClient;
		private const string user = "testuser";
		private const string pass = "testpass";

		private const string app_id_fake = "abcdefg";
		private const string app_secret_fake = "0123456789abcdef";

		private const string app_id = "kid_Zy0JOYPKkZ";
		private const string app_secret = "d83de70e64d540e49acd6cfce31415df";

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
			const string url = "https://baas.kinvey.com/";
			Client.Builder builder = new Client.Builder(app_id, app_secret);

			// Act
			Client client = builder.build();

			// Assert
			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
			Assert.True(string.Equals(client.BaseUrl, url));
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public void TestClientBuilderBasicBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public void TestClientBuilderSetValues()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_id, app_secret);

			// Act
			builder.setFilePath("")
				.setLogger(delegate(string msg) { Console.WriteLine(msg); });

			// Assert
			Client client = builder.build();

			Assert.False(client == null);
			Assert.False(string.IsNullOrEmpty(client.BaseUrl));
			Assert.False(client.Store == null);
			Assert.False(client.logger == null);
			Assert.False(string.IsNullOrEmpty(client.MICHostName));
		}

		[Test]
		public void ClientBuilderSetBaseURL()
		{
			// Arrange
			const string url = "https://www.test.com/";
			Client.Builder builder = new Client.Builder(app_id, app_secret);

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
			Client.Builder builder = new Client.Builder(app_id, app_secret);

			// Act
			// Assert
			Assert.Catch( delegate() {
				builder.setBaseURL("www.test.com");
			});
		}

		[Test]
		public async Task TestClientPingAsync()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_id, app_secret);
			Client client = builder.build();

			// Act
			PingResponse pr = await client.PingAsync();

			// Assert
			Assert.IsNotNullOrEmpty(pr.kinvey);
			Assert.True(pr.kinvey.StartsWith("hello"));
			Assert.IsNotNullOrEmpty(pr.version);
		}

		[Test]
		public async Task TestClientPingAsyncBad()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(app_id_fake, app_secret_fake);
			Client client = builder.build();

			// Act
			PingResponse pr = await client.PingAsync();

			// Assert
			Assert.IsNullOrEmpty(pr.kinvey);
			Assert.IsNullOrEmpty(pr.version);
		}
	}
}
