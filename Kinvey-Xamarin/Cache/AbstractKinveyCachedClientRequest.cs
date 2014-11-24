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
using System.Collections.Generic;

namespace KinveyXamarin
{
	public class AbstractKinveyCachedClientRequest<T> : AbstractKinveyOfflineClientRequest<T>
	{

		private CachePolicy cachePolicy = CachePolicy.NO_CACHE;
		// TODO no anonymous interface support in c# -> investigate making ICache an abstract class
		private Cache<String, T> cache = null;



		private Object locker = new Object();


		protected AbstractKinveyCachedClientRequest(AbstractKinveyClient client, string requestMethod, string uriTemplate, T httpContent, Dictionary<string, string> uriParameters, string collection) 
			: base (client, requestMethod, uriTemplate, httpContent, uriParameters, collection)
		{
		}

		public void setCache(Cache<String, T> cache, CachePolicy policy)
		{
			this.cache = cache;
			this.cachePolicy = policy;
		}

//		public void setCache(ICache<String, T[]> cache, CachePolicy policy){
//
//		}


		public T fromCache(){
			if (cache == null) {
				return default(T);
			}

			var key = base.uriTemplate;
			foreach (var p in base.uriResourceParameters)
			{
				key = key.Replace("{" + p.Key + "}", p.Value.ToString());
			}
				
			lock (locker) {
				return this.cache.get (key);
			}
		}

		public T fromService(bool persist){
			T ret = base.Execute ();
			if (persist && ret != null && cache != null) {

				var key = base.uriTemplate;
				foreach (var p in base.uriResourceParameters)
				{
					key = key.Replace("{" + p.Key + "}", p.Value.ToString());
				}

				lock (locker) {
					this.cache.put (key, ret);
				}	
			}
			return ret;
		}

		public override T Execute ()
		{
			T ret = default(T);

			if (cachePolicy == CachePolicy.NO_CACHE) {
				ret = fromService (false);

			} else if (cachePolicy == CachePolicy.CACHE_ONLY) {
				ret = fromCache ();

			} else if (cachePolicy == CachePolicy.CACHE_FIRST) {
				ret = fromCache ();
				if (ret == null) {
					ret = fromService (true);
				}
			} else if (cachePolicy == CachePolicy.CACHE_FIRST_NOREFRESH) {
				ret = fromCache ();
				if (ret == null) {
					ret = fromService(false);
				}
			} else if (cachePolicy == CachePolicy.NETWORK_FIRST) {
				ret = fromService (true);
				if (ret == null) {
					ret = fromCache ();
				}
				
			}

			return ret;

		}




	}
}

