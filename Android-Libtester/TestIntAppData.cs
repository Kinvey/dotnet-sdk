using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Reflection;
using RestSharp;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestIntAppData
	{

		private string kid = "kid_-yHYO8aYY";
		private string secret = "4c2f4856a6c54573b9fccd05c293a58e";
		private string collection = "TestData";
		private string someID = "test";

		private Client client;
		//private AsyncAppData<MyEntity> appData;
		private AppData<MyEntity> appData;

		[SetUp]
		public void Setup ()
		{
			client = new Client.Builder (kid, secret).build ();
			appData = client.AppData<MyEntity> (collection, typeof(MyEntity));
		}


		[Test()]
		public async void TestSaveEntity()
		{
			MyEntity ent = new MyEntity ();
			ent.Email = "test@test.com";
			ent.IsAvailable = true;
			ent.lowercasetest = "UpperCase";
			ent.Name = "James Dean";

			MyEntity saved = await appData.SaveAsync (ent);
	
			Assert.Equals (ent.Name, saved.Name);
			Assert.Equals (ent.lowercasetest, saved.lowercasetest);
			Assert.Equals (ent.IsAvailable, saved.IsAvailable);
			Assert.Equals (ent.Email, saved.Email);
		}

		[Test()]
		public async void TestGetEntity(){
			await appData.SaveAsync (new MyEntity (someID));

			MyEntity ret = await appData.GetEntityAsync (someID);

			Assert.Equals (ret.ID, someID);

		}




	}
}

