using System;
using SQLite.Net.Interop;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	public class BackgroundExecutor<T>
	{

		private ISQLitePlatform platform;
		private string dbpath;
		private Client client;

		public BackgroundExecutor (Client c)
		{
			this.dbpath = Path.Combine(c.filePath,  "kinveyOffline.sqlite") ;
			this.platform = c.offline_platform;
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
				appdata.Get (id, new KinveyDelegate<T[]>{ 
					onSuccess = (results) => { 

						List<string> idresults = new List<string>();

						foreach ( T ent in results){
						
							string entJSON = JsonConvert.SerializeObject(ent);
							string entID = JObject.FromObject (ent) ["_id"].ToString();

							handler.upsertEntity(entID, collection, entJSON);

							idresults.Add(entID);
						}

						handler.saveQueryResults(id, collection, idresults);

						handler.removeFromQueue(item.key);
						doneSuccessfully();

					},
					onError = (error) => {
						ClientLogger.Log(error);
					}
				
				});
				break;
			case "PUT":
				T entity = handler.getEntity (collection, id);

				appdata.Save (entity, new KinveyDelegate<T> { 
					onSuccess = (T) => { 
						string json = JsonConvert.SerializeObject(T);
						handler.upsertEntity(id, collection, json);
						handler.removeFromQueue(item.key);
						doneSuccessfully();
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
						doneSuccessfully();
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

		private void doneSuccessfully(){
			Task.Run (() => {
				new BackgroundExecutor<T> ((Client)client).RunSync ();
			});
		}



		private DatabaseHelper<T> getDatabaseHelper(){
			return SQLiteHelper<T>.getInstance (platform, dbpath);
		}
	}
}

