// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using SQLite.Net.Interop;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using KinveyUtils;

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
			RunSync ();
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

			SQLTemplates.QueueItem req = handler.popQueueAsync ().Result;

			if (req != null) {
				buildAndExecuteRequest (handler, req);
			}
		}


		/// <summary>
		/// Builds the queued up request and updates it.
		/// </summary>
		/// <param name="handler">Handler to access database.</param>
		/// <param name="item">The queued representation of the request.</param>
		private async void buildAndExecuteRequest(DatabaseHelper<T> handler, SQLTemplates.QueueItem item){
			string collection = item.collection;
			string verb = item.action;
			SQLTemplates.OfflineMetaData meta = JsonConvert.DeserializeObject<SQLTemplates.OfflineMetaData>(item.OfflineMetaDataAsJson);

			AsyncAppData<T> appdata = client.AppData<T> (collection, typeof(T));

			switch (verb) {
			case "QUERY":
				
				appdata.SetCustomRequestProperties (meta.customHeaders);
				appdata.SetClientAppVersion (meta.clientVersion);
				T[] results = default(T[]);
				try{
					results = await appdata.GetAsync (meta.id);
				}catch(Exception error){
					if (!((KinveyJsonResponseException) error).Details.Debug.Contains("InsufficientCredentials")){
						handler.enqueueRequestAsync("QUERY", collection, meta);
					}
					Logger.Log(error);
				}

				List<string> idresults = new List<string>();
				foreach ( T ent in results){

					string entJSON = JsonConvert.SerializeObject(ent);
					string entID = JObject.FromObject (ent) ["_id"].ToString();

					await handler.upsertEntityAsync(entID, collection, entJSON);

					idresults.Add(entID);
				}
				await handler.saveQueryResultsAsync(meta.id, collection, idresults);

				doneSuccessfully();

				break;
			case "PUT":


				T entity = handler.getEntityAsync (collection, meta.id).Result;
				appdata.SetCustomRequestProperties (meta.customHeaders);
				appdata.SetClientAppVersion (meta.clientVersion);
				appdata.Save (entity, new KinveyDelegate<T> { 
					onSuccess = (T) => { 
						string json = JsonConvert.SerializeObject(T);
						handler.upsertEntityAsync(meta.id, collection, json);
						doneSuccessfully();
					},
					onError = (error) => {
						if (!((KinveyJsonResponseException) error).Details.Error.Contains("InsufficientCredentials")){
							handler.enqueueRequestAsync("PUT", collection, meta);
						}
						Logger.Log(error);
					}
				});


				break;
			case "GET":
				appdata.SetCustomRequestProperties (meta.customHeaders);
				appdata.SetClientAppVersion (meta.clientVersion);
				appdata.GetEntity (meta.id, new KinveyDelegate<T> { 
					onSuccess = (T) => { 
						string json = JsonConvert.SerializeObject(T);
						handler.upsertEntityAsync(meta.id, collection, json);
						doneSuccessfully();
					},
					onError = (error) => {
						if (!((KinveyJsonResponseException) error).Details.Debug.Contains("InsufficientCredentials")){
							handler.enqueueRequestAsync("GET", collection, meta);
						}
						Logger.Log(error);					
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

