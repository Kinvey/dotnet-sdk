using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Threading.Tasks;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestAppData
	{
		private Client kinveyClient;
		private const string user = "testuser";
		private const string pass = "testpass";
		private const string app_key = "abcdefg";
		private const string app_secret = "0123456789abcdef";

		[SetUp]
		public void Setup ()
		{
			KinveyDelegate<User> delegates = new KinveyDelegate<User>();
			kinveyClient = new Client.Builder(app_key, app_secret).build();
		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetEntityAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetEntityAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}
	}
}
