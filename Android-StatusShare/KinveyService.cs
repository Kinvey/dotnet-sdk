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

		public static void saveUpdate(UpdateEntity entity, byte[] bytes, KinveyDelegate<UpdateEntity> delegates){
			AsyncAppData<UpdateEntity> appData = getClient().AppData<UpdateEntity> (update_collection, typeof(UpdateEntity));

			FileMetaData fm  = new FileMetaData();
			fm.acl = entity.acl;
			fm._public = true;

			getClient().File().upload(fm, bytes, new KinveyDelegate<FileMetaData>{ 
				onSuccess =  (meta) => { 

					entity.attachement = new KinveyFile(meta.id);
					appData.Save(entity, delegates);

				},
				onError = (error) => {

				}
			});


		}

		public static string getCurrentUserId(){
			return getClient ().User ().Id;
		}
	}
}

