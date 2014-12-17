using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using SQLite.Net.Interop;

namespace KinveyXamarin
{
	/// <summary>
	/// This is an implementation of an OfflineStore, using SQLite to manage maintaining data.
	/// This class is responsible for breaking apart a request, and determing what actions to take
	/// Actual actions are performed on the OfflineTable class, using a SQLiteDatabaseHelper
	/// </summary>
	public class SQLiteOfflineStore : IOfflineStore
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteOfflineStore"/> class.
		/// </summary>
		public SQLiteOfflineStore ()
		{
		}

		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		/// <value>The platform.</value>
		public ISQLitePlatform platform {get; set;}

		/// <summary>
		/// Gets or sets the database file path.
		/// </summary>
		/// <value>The dbpath.</value>
		public string dbpath{ get; set;}

		/// <summary>
		/// Executes a get request.
		/// </summary>
		/// <returns>The response object.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		/// <param name="appData">App data.</param>
		public object executeGet<T>(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper<T> ();

			//expand the URL
			string targetURI = request.uriTemplate;
			foreach (var p in request.uriResourceParameters)
			{
				targetURI = targetURI.Replace("{" + p.Key + "}", p.Value.ToString());
			}

			int idIndex = targetURI.IndexOf(appData.CollectionName) + appData.CollectionName.Length + 1;



			object ret = null;
			//is it a query?  (12 is magic number for decoding empty query string)
			if (targetURI.Contains ("query") && (targetURI.IndexOf ("query") + 12) != targetURI.Length) {
			
				//it's a query!
				//pull the actual query string out and get rid of the "?query"
				String query = targetURI.Substring(idIndex, targetURI.Length - idIndex);
				query = query.Replace("?query=","");
				query = WebUtility.UrlDecode(query);

				handler.createTable (appData.CollectionName);

				T[] ok = handler.getQuery(appData.CollectionName,  query);


				handler.enqueueRequest("QUERY", appData.CollectionName, query);
				return ok;

			} else if (idIndex == (targetURI.Length + 1)|| targetURI.Contains ("query")) {
				//it's a get all request (no query, no id)
				handler.createTable (appData.CollectionName);
				List<T> ok = handler.getAll (appData.CollectionName);
				return ok;
			} else {
				//it's a get by id
				String targetID = targetURI.Substring(idIndex, targetURI.Length - idIndex);
				handler.createTable (appData.CollectionName);
				ret = (T)handler.getEntity (appData.CollectionName, targetID);

				handler.enqueueRequest("GET", appData.CollectionName, targetURI.Substring(idIndex, targetURI.Length - idIndex));
			}


			return ret;
		}

		/// <summary>
		/// Executes a save request.
		/// </summary>
		/// <returns>The save.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		/// <param name="appData">App data.</param>
		public object executeSave<T>(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper<T> ();

			//grab json content and put it in the store
			string jsonContent = null;
			if (request.HttpContent != null) {
				jsonContent = JsonConvert.SerializeObject (request.HttpContent);
			}
		

			//grab the ID
			JToken token = JObject.Parse(jsonContent);
			string id = (string)token.SelectToken("_id");

			//insert the entity into the database
			handler.createTable (appData.CollectionName);
			handler.upsertEntity(id, appData.CollectionName, jsonContent);
			//enque the request
			handler.enqueueRequest("PUT", appData.CollectionName, id);

			return request.HttpContent;
		}

		/// <summary>
		/// Executes a delete request.
		/// </summary>
		/// <returns>The delete.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		/// <param name="appData">App data.</param>
		public KinveyDeleteResponse executeDelete<T>(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper<T> ();

			//expand the URL
			string targetURI = request.uriTemplate;
			foreach (var p in request.uriResourceParameters)
			{
				targetURI = targetURI.Replace("{" + p.Key + "}", p.Value.ToString());
			}
			int idIndex = targetURI.IndexOf(appData.CollectionName) + appData.CollectionName.Length + 1;

			String targetID = targetURI.Substring(idIndex, targetURI.Length - idIndex);




			handler.createTable (appData.CollectionName);
			KinveyDeleteResponse ret = handler.delete(appData.CollectionName, targetID);

			handler.enqueueRequest("DELETE",appData.CollectionName, targetURI.Substring(idIndex, targetURI.Length - idIndex));
			return ret;
		}

		/// <summary>
		/// Inserts an entity directly into the database.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		/// <param name="appData">App data.</param>
		public void insertEntity<T>(AbstractKinveyClient client, AppData<T> appData, T entity){

			DatabaseHelper<T> handler = getDatabaseHelper<T> ();

			string jsonContent = JsonConvert.SerializeObject (entity);

			//grab the ID
			JToken token = JObject.Parse(jsonContent);
			string id = (string)token.SelectToken("_id");

			handler.createTable (appData.CollectionName);

			handler.upsertEntity( id, appData.CollectionName, jsonContent);

		}

		/// <summary>
		/// Clears the storage.
		/// </summary>
		public void clearStorage(){
			//TODO
		}
			

		/// <summary>
		/// Gets the database helper.
		/// </summary>
		/// <returns>The database helper.</returns>
		/// <typeparam name="T">The type of entities stored in this collection.</typeparam>
		private DatabaseHelper<T> getDatabaseHelper<T>(){
			return SQLiteHelper<T>.getInstance (platform, dbpath);
		}

	}
}

