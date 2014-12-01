using System;
using SQLite.Net.Interop;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{

	/// <summary>
	/// This class is used by the offline implementation to pop the queue and fire off requests.
	/// </summary>
	public class BackgroundExecutor<T>
	{

		/// <summary>
		/// The platform.
		/// </summary>
		private ISQLitePlatform platform;
		/// <summary>
		/// The location of the database..
		/// </summary>
		private string dbpath;
		/// <summary>
		/// The Kinvey client.
		/// </summary>
		private Client client;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.BackgroundExecutor`1"/> class.
		/// </summary>
		/// <param name="client">The Kinvey client</param>
		public BackgroundExecutor (Client client)
		{
			this.dbpath = Path.Combine(client.filePath,  "kinveyOffline.sqlite") ;
			this.platform = client.offline_platform;
			this.client = client;
		}

		/// <summary>
		/// runs sync
		/// </summary>
		public void RunSync(){

			getFromStoreAndExecute ();


		}

		/// <summary>
		/// Gets a request from the offline store associated with the client, and executes it.
		/// </summary>
		private void getFromStoreAndExecute(){
			DatabaseHelper<T> handler = getDatabaseHelper ();

			SQLTemplates.QueueItem req = handler.popQueue ();

			if (req != null) {
				buildAndExecuteRequest (handler, req);
			}



		}


		/// <summary>
		/// Builds the queued up request and updates it.
		/// </summary>
		/// <param name="handler">Handler to access database.</param>
		/// <param name="item">The queued representation of the request.</param>
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

		/// <summary>
		/// Executed when a request has completely successfully
		/// </summary>
		private void doneSuccessfully(){
			Task.Run (() => {
				new BackgroundExecutor<T> ((Client)client).RunSync ();
			});
		}


		/// <summary>
		/// returns an instance of the database helper.
		/// </summary>
		/// <returns>The database helper.</returns>
		private DatabaseHelper<T> getDatabaseHelper(){
			return SQLiteHelper<T>.getInstance (platform, dbpath);
		}
	}
}

