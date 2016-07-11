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

//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace KinveyXamarin
//{
//	/// <summary>
//	/// This client request object adds support for pulling respones from a cache.
//	/// </summary>
//	public class AbstractKinveyCachedClientRequest<T> : AbstractKinveyOfflineClientRequest<T>
//	{
//		/// <summary>
//		/// The cache policy.
//		/// </summary>
//		private ReadPolicy cachePolicy = ReadPolicy.FORCE_NETWORK;
//		// TODO no anonymous interface support in c# -> investigate making ICache an abstract class
//		/// <summary>
//		/// The cache itself.
//		/// </summary>
//		private Cache<String, T> cache = null;
//		/// <summary>
//		/// The lock which cache access is syncronized on.
//		/// </summary>
//		private Object locker = new Object();

//		/// <summary>
//		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyCachedClientRequest`1"/> class.
//		/// </summary>
//		/// <param name="client">Client.</param>
//		/// <param name="requestMethod">Request method.</param>
//		/// <param name="uriTemplate">URI template.</param>
//		/// <param name="httpContent">Http content.</param>
//		/// <param name="uriParameters">URI parameters.</param>
//		/// <param name="collection">Collection.</param>
//		protected AbstractKinveyCachedClientRequest(AbstractClient client, string requestMethod, string uriTemplate, T httpContent, Dictionary<string, string> uriParameters, string collection) 
//			: base (client, requestMethod, uriTemplate, httpContent, uriParameters, collection)
//		{
//		}

//		/// <summary>
//		/// Sets the cache.
//		/// </summary>
//		/// <param name="cache">Cache.</param>
//		/// <param name="policy">Policy.</param>
//		public void setCache(Cache<String, T> cache, ReadPolicy policy)
//		{
//			this.cache = cache;
//			this.cachePolicy = policy;
//		}
			
//		/// <summary>
//		/// Pulls a value from the cache, or the default value if it's not present.
//		/// </summary>
//		/// <returns>The cache.</returns>
//		public T fromCache(){
//			if (cache == null) {
//				return default(T);
//			}

//			var key = base.uriTemplate;
//			foreach (var p in base.uriResourceParameters)
//			{
//				key = key.Replace("{" + p.Key + "}", p.Value.ToString());
//			}
				
//			lock (locker) {
//				return this.cache.get (key);
//			}
//		}

//		/// <summary>
//		/// Froms the service.
//		/// </summary>
//		/// <returns>The service.</returns>
//		/// <param name="persist">If set to <c>true</c> persist.</param>
//		public T fromService(bool persist){
//			T ret = base.Execute ();
//			if (persist && ret != null && cache != null) {

//				var key = base.uriTemplate;
//				foreach (var p in base.uriResourceParameters)
//				{
//					key = key.Replace("{" + p.Key + "}", p.Value.ToString());
//				}

//				lock (locker) {
//					this.cache.put (key, ret);
//				}	
//			}
//			return ret;
//		}

//		/// <summary>
//		/// Froms the service.
//		/// </summary>
//		/// <returns>The service.</returns>
//		/// <param name="persist">If set to <c>true</c> persist.</param>
//		public async Task<T> fromServiceAsync(bool persist){
//			T ret = await base.ExecuteAsync ();
//			if (persist && ret != null && cache != null) {

//				var key = base.uriTemplate;
//				foreach (var p in base.uriResourceParameters)
//				{
//					key = key.Replace("{" + p.Key + "}", p.Value.ToString());
//				}

//				lock (locker) {
//					this.cache.put (key, ret);
//				}	
//			}
//			return ret;
//		}

//		/// <summary>
//		/// Execute this request.
//		/// </summary>
//		public override T Execute ()
//		{
//			T ret = default(T);

//			if (cachePolicy == ReadPolicy.FORCE_NETWORK) {
//				ret = fromService (false);

//			} else if (cachePolicy == ReadPolicy.FORCE_LOCAL) {
//				ret = fromCache ();

//			} else if (cachePolicy == ReadPolicy.BOTH) {
//				ret = fromCache ();
//				if (ret == null) {
//					ret = fromService (true);
//				}
//			}

//			return ret;

//		}

//		public async override Task<T> ExecuteAsync(){

//			T ret = default(T);

//			if (cachePolicy == ReadPolicy.FORCE_NETWORK) {
//				ret = await fromServiceAsync (false);

//			} else if (cachePolicy == ReadPolicy.FORCE_LOCAL) {
//				ret = fromCache ();

//			} else if (cachePolicy == ReadPolicy.BOTH) {
//				ret = fromCache ();
//				if (ret == null) {
//					ret = await fromServiceAsync (true);
//				}
//			}

//			return ret;

//		}




//	}
//}

