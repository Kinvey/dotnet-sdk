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
		private User activeUser;

		[SetUp]
		public void Setup ()
		{
			KinveyDelegate<User> delegates = new KinveyDelegate<User>();
			kinveyClient = new Client.Builder("app_key", "app_secret").build();
		}

		[TearDown]
		public void Tear ()
		{
		}

	}
}
