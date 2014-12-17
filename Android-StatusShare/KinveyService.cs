using System;
using KinveyXamarin;

namespace AndroidStatusShare
{
	public class KinveyService
	{


		private static Client client;

		private static string update_collection = "Updates";

		public static Client getClient(){
			if (client == null){
				client = new Client.Builder("kid_W1YYO1eOv","16e676441fe54f9994e565f637e16a21")
					.setLogger(delegate(string msg) { Console.WriteLine(msg);})
					.build();
			}
			return client;
		}


		public static void login(string username, string password, KinveyDelegate<User> delegates){
			getClient ().User ().Login (username, password, delegates);
		}

		public static void register(string username, string password, KinveyDelegate<User> delegates){
			getClient ().User ().Create (username, password, delegates);
		}

		public static void getUpdates(KinveyDelegate<UpdateEntity[]> entities){

			AsyncAppData<UpdateEntity> appData = getClient().AppData<UpdateEntity> (update_collection, typeof(UpdateEntity));

			appData.Get (entities);

		}

		public static void logout(){
			getClient ().User ().Logout ();
		}

		public static void saveUpdate(UpdateEntity entity, byte[] bytes, KinveyDelegate<UpdateEntity> delegates, KinveyDelegate<FileMetaData> fileDelegate){
			AsyncAppData<UpdateEntity> appData = getClient().AppData<UpdateEntity> (update_collection, typeof(UpdateEntity));

			appData.Save(entity, delegates);

			getClient().File().upload(new FileMetaData(), bytes, fileDelegate);


		}





	}
}

