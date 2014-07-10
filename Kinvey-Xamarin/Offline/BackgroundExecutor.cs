using System;
using SQLite.Net.Interop;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	public class BackgroundExecutor<T>
	{

		private ISQLitePlatform platform;
		private string dbpath;
		private Client client;

		public BackgroundExecutor (Client c, ISQLitePlatform platform, string dbpath)
		{
			this.dbpath = dbpath;
			this.platform = platform;
			this.client = c;
		}


		public void RunSync(){

			getFromStoreAndExecute ();


		}


		private void getFromStoreAndExecute(){
			DatabaseHelper<T> handler = getDatabaseHelper ();

			SQLTemplates.QueueItem req = handler.popQueue ();

			if (req != null) {
				buildAndExecuteRequest (handler, req);
			}



		}


		private void buildAndExecuteRequest(DatabaseHelper<T> handler, SQLTemplates.QueueItem item){
			string collection = item.collection;
			string verb = item.action;
			string id = item.id;
			AsyncAppData<T> appdata = client.AppData<T> (collection, typeof(T));

			switch (verb) {
			case "QUERY":


				break;
			case "PUT":
				T entity = handler.getEntity (collection, id);

				appdata.Save (entity, new KinveyDelegate<T> { 
					onSuccess = (T) => { 
						string json = JsonConvert.SerializeObject(T);
						handler.upsertEntity(id, collection, json);
						handler.removeFromQueue(item.key);
					},
					onError = (error) => {
						ClientLogger.Log(error);
					}
				});


				break;
			case "GET":
				appdata.GetEntity (id, new KinveyDelegate<T> { 
					onSuccess = (T) => { 
						string json = JsonConvert.SerializeObject(T);
						handler.upsertEntity(id, collection, json);
						handler.removeFromQueue(item.key);
					},
					onError = (error) => {
						ClientLogger.Log(error);
					}
				});


				break;
			case "DELETE":
//				appdata.Delete (entity, new KinveyDelegate<T> { 
//					onSuccess = (T) => { 
//
//						handler.removeFromQueue(item.key);
//					},
//					onError = (error) => {
//						ClientLogger.Log(error);
//					}
//				});


				break;
			

			



			}

		}




		private DatabaseHelper<T> getDatabaseHelper(){
			return SQLiteHelper<T>.getInstance (platform, dbpath);
		}
	}
}

