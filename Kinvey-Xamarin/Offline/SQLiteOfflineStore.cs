using System;
using Kinvey.DotNet.Framework.Core;
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
	public class SQLiteOfflineStore<T> : IOfflineStore<T>
	{
		public SQLiteOfflineStore ()
		{
		}

		public ISQLitePlatform platform {get; set;}
		public string dbpath{ get; set;}

		public T executeGet(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper ();

			//expand the URL
			string targetURI = request.uriTemplate;
			foreach (var p in request.uriResourceParameters)
			{
				targetURI = targetURI.Replace("{" + p.Key + "}", p.Value.ToString());
			}

			int idIndex = targetURI.IndexOf(appData.CollectionName) + appData.CollectionName.Length + 1;



			T ret = default(T);
			//is it a query?  (12 is magic number for decoding empty query string)
			if (targetURI.Contains ("query") && (targetURI.IndexOf ("query") + 12) != targetURI.Length) {
				//it's a query!
				//pull the actual query string out and get rid of the "?query"
				String query = targetURI.Substring(idIndex, targetURI.Length - idIndex);
				query = query.Replace("?query=","");
				query = WebUtility.UrlDecode(query);

				handler.createTable (appData.CollectionName);

				List<T> ok = handler.getQuery(appData.CollectionName,  query);
				throw new NotImplementedException ();

				handler.enqueueRequest("QUERY", appData.CollectionName, query);

			} else if (idIndex == targetURI.Length || targetURI.Contains ("query")) {
				//it's a get all request (no query, no id)
				List<T> ok = handler.getAll (appData.CollectionName);
				throw new NotImplementedException ();

			} else {
				//it's a get by id
				String targetID = targetURI.Substring(idIndex, targetURI.Length - idIndex);
				ret = (T)handler.getEntity (appData.CollectionName, targetID);

				handler.enqueueRequest("GET", appData.CollectionName, targetURI.Substring(idIndex, targetURI.Length - idIndex));
			}

			kickOffSync ();

			return ret;
		}

		public T executeSave(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper ();

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

			kickOffSync();

			return request.HttpContent;
		}

		public KinveyDeleteResponse executeDelete(AbstractKinveyClient client, AppData<T> appData, AbstractKinveyOfflineClientRequest<T> request){
			DatabaseHelper<T> handler = getDatabaseHelper ();

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
			throw new NotImplementedException ();
			kickOffSync();
			return ret;
		}

		public void insertEntity(AbstractKinveyClient client, AppData<T> appData, T entity){

			DatabaseHelper<T> handler = getDatabaseHelper ();

			string jsonContent = JsonConvert.SerializeObject (entity);

			//grab the ID
			JToken token = JObject.Parse(jsonContent);
			string id = (string)token.SelectToken("_id");

			handler.createTable (appData.CollectionName);

			handler.upsertEntity( id, appData.CollectionName, jsonContent);

		}

		public void clearStorage(){
			//TODO
		}

		public void kickOffSync(){
			//TODO sync
		}

		private DatabaseHelper<T> getDatabaseHelper(){
			return SQLiteHelper<T>.getInstance (platform, dbpath);
		}

	}
}

