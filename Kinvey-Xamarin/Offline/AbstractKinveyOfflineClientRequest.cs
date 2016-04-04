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
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using KinveyUtils;
using System.Reflection;

namespace KinveyXamarin
{
	/// <summary>
	/// This class represents a Kinvey request that can be executed offline.
	/// </summary>
	public class AbstractKinveyOfflineClientRequest<T> : AbstractKinveyClientRequest<T>
	{
		/// <summary>
		/// The cache.
		/// </summary>
		private ICache<T> cache;
		/// <summary>
		/// The policy.
		/// </summary>
		private ReadPolicy policy = ReadPolicy.FORCE_NETWORK;

		/// <summary>
		/// The name of the collection.
		/// </summary>
		private string collectionName;


		protected AbstractKinveyOfflineClientRequest(AbstractClient client, string requestMethod, string uriTemplate, T httpContent, Dictionary<string, string> uriParameters, string collection) 
			: base (client, requestMethod, uriTemplate, httpContent, uriParameters)
		{
			this.collectionName = collection;
		}

		/// <summary>
		/// Sets the cache.
		/// </summary>
		/// <param name="newStore">the offline cache to use.</param>
		/// <param name="newPolicy">the offline policy to use.</param>
		public void SetCache(ICache<T> newStore, ReadPolicy newPolicy){
			this.cache = newStore;
			this.policy = newPolicy;
		}

		/// <summary>
		/// Executes the request from the offline cache
		/// </summary>
		/// <returns>The response, if there is one, from the offline cache.</returns>
		public async Task<T> offlineFromStoreAsync(){
			return default(T);
//			if (cache == null) {
//				return default(T);
//			}
//
//			string verb = base.RequestMethod;
//			T ret = default(T);
//
//			if (verb.Equals ("GET")) {
//				ret = (T)cache.GetAsync (this).Result;
//			} else if (verb.Equals ("PUT")) {
//				ret = (T) cache.SaveAsync (this).Result;
//
//			} else if (verb.Equals ("POST")) {
//				JObject jobj = JObject.FromObject (this.HttpContent);
//				jobj["_id"] = getGUID ();
//				this.HttpContent = jobj.ToObject<T>();
//
//				ret = (T)cache.SaveAsync (this).Result;
//
//			} else if (verb.Equals ("DELETE")) {
//				KinveyDeleteResponse resp = cache.DeleteAsync (this).Result;
//				//					return resp;
//				//TODO
//				return default(T);
//			}
//
//			return ret;
		}

		/// <summary>
		/// Executes the request over the network
		/// </summary>
		/// <returns>The entity from the service.</returns>
		public T offlineFromService(){
			T ret = base.Execute ();
			return ret;
		}

		/// <summary>
		/// Executes the request over the network using async/await
		/// </summary>
		/// <returns>The from service async.</returns>
		public async Task<T> offlineFromServiceAsync(){
			var ret = await base.ExecuteAsync ();
			return ret;
		}


		/// <summary>
		/// Execute this request.
		/// </summary>
		public override T Execute(){
			T ret =  default(T);
//
//			if (policy == ReadPolicy.FORCE_NETWORK) {
//				ret = offlineFromService ();
//
//			} else if (policy == ReadPolicy.FORCE_LOCAL) {
//				ret = offlineFromStore ();
//			}
//			else if (policy == ReadPolicy.BOTH) {
//				ret = offlineFromStore ();
//				if (ret == null) {
//					ret = offlineFromService ();
//				}
//
//			} 
//			kickOffSync ();
//
			return ret;
		}

		public async override Task<T> ExecuteAsync(){
			T ret =  default(T);

			if (policy == ReadPolicy.FORCE_NETWORK) {
				ret = await offlineFromServiceAsync ();

			} else if (policy == ReadPolicy.FORCE_LOCAL) {
				ret = await offlineFromStoreAsync ();

			} else if (policy == ReadPolicy.BOTH) {
				ret = await offlineFromStoreAsync ();
				if (ret == null) {
					ret = await offlineFromServiceAsync ();
				}

			}

			//kickOffSync ();

			return ret;

		}
	
		/// <summary>
		/// Kicks off the background sync thread
		/// </summary>
		public void kickOffSync(){
//			Type parameterType = typeof(T);
//			if (parameterType.IsArray) {
//				parameterType = parameterType.GetElementType ();
//			}

			Type parameterType = typeof(T);
			Type executor = typeof(BackgroundExecutor<>);
			Type gen = executor.MakeGenericType (parameterType);

			foreach (var ctor in gen.GetTypeInfo().DeclaredConstructors) {
				Task.Run (() => {
					ctor.Invoke (new object[1]{ (Client)Client });
				});
			}
		}

		/// <summary>
		/// get a unique guid for this device
		/// </summary>
		/// <returns>The GUID</returns>
		private string getGUID(){
			string guid =  Guid.NewGuid().ToString();
			return guid.Replace ("-", "");
		}
	}
}

