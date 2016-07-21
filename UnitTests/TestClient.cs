using System;
using System.Threading.Tasks;
using NUnit.Framework;
using KinveyXamarin;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestClient
	{
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
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);

			// Act
			Client client = builder.build();

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
			Assert.Catch (delegate () {
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
			Assert.Catch( delegate() {
				builder.setBaseURL("www.test.com");
			});
		}

		[Test]
		public async Task TestClientPingAsync()
		{
			// Arrange
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret);
			Client client = builder.build();

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
			Client client = builder.build();

			// Act
			PingResponse pr = await client.PingAsync();

			// Assert
			Assert.IsNotNull(pr);
			Assert.IsNull(pr.kinvey);
			Assert.IsNull(pr.version);
		}
	}
}
