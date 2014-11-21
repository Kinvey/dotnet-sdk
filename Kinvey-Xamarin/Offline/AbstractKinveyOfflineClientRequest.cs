// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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
using Kinvey.DotNet.Framework.Core;
using System.Collections.Generic;
using Kinvey.DotNet.Framework;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	public class AbstractKinveyOfflineClientRequest<T> : AbstractKinveyClientRequest<T>
	{

		private IOfflineStore store;
		private OfflinePolicy policy = OfflinePolicy.ALWAYS_ONLINE;

		private Object locker = new Object();
		private string collectionName;


		protected AbstractKinveyOfflineClientRequest(AbstractKinveyClient client, string requestMethod, string uriTemplate, T httpContent, Dictionary<string, string> uriParameters, string collection) 
			: base (client, requestMethod, uriTemplate, httpContent, uriParameters)
		{
			this.collectionName = collection;
		}

		public void SetStore(IOfflineStore newStore, OfflinePolicy newPolicy){
			this.store = newStore;
			this.policy = newPolicy;
		}

		public T offlineFromStore(){
			if (store == null) {
				return default(T);
			}

			string verb = base.RequestMethod;
			T ret = default(T);

			if (verb.Equals ("GET")) {
				lock (locker) {
					ret = (T) store.executeGet ((AbstractClient)(client), ((AbstractClient)client).AppData<T>(collectionName, typeof(T)), this);
				}
			} else if (verb.Equals ("PUT")) {
				lock (locker) {
					ret = (T) store.executeSave ((AbstractClient)(client), ((AbstractClient)client).AppData<T>(collectionName, typeof(T)), this);
				}
			} else if (verb.Equals ("POST")) {
				lock (locker) {
					JObject jobj = JObject.FromObject (this.HttpContent);
					jobj["_id"] = getGUID ();
					this.HttpContent = jobj.ToObject<T>();

					ret = (T) store.executeSave ((AbstractClient)(client), ((AbstractClient)client).AppData<T>(collectionName, typeof(T)), this);
				}
			} else if (verb.Equals ("DELETE")) {
				lock (locker) {

					KinveyDeleteResponse resp = store.executeDelete ((AbstractClient)(client), ((AbstractClient)client).AppData<T>(collectionName, typeof(T)), this);
//					return resp;
					//TODO
					return default(T);
				}
			}

			return ret;
		}

		public T offlineFromService(){
			T ret = base.Execute ();
			return ret;
		}



		public override T Execute(){
			T ret =  default(T);

			if (policy == OfflinePolicy.ALWAYS_ONLINE) {
				ret = offlineFromService ();

			} else if (policy == OfflinePolicy.LOCAL_FIRST) {
				ret = offlineFromStore ();
				if (ret == null) {
					try {
						ret = offlineFromService ();
					} catch (Exception e) {
						ClientLogger.Log (e);
					}
				}

			} else if (policy == OfflinePolicy.ONLINE_FIRST) {
				try {
					ret = offlineFromService ();
				} catch (Exception e) {
					ClientLogger.Log (e);
					ret = offlineFromStore ();
				}
			}

			kickOffSync ();

			return ret;
		}
	

		public void kickOffSync(){
			Task.Run (() => {
				new BackgroundExecutor<T> ((Client)Client).RunSync ();
			});
		}

		private string getGUID(){
			string guid =  Guid.NewGuid().ToString();
			return guid.Replace ("-", "");
		}
	}
}

