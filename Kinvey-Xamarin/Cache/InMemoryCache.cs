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
	/// <summary>
	/// An implementation of an in memory cache, backed by a Dictionary.
	/// </summary>
	public class InMemoryCache<V> : Cache<String, V>
	{
		/// <summary>
		/// The cache itself.
		/// </summary>
		private Dictionary<String, V> cache;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.InMemoryCache`1"/> class.
		/// </summary>
		public InMemoryCache (){
			cache = new Dictionary<String, V> ();
		}

		/// <summary>
		/// Put the specified key and value into the cache
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void put(String key, V value){
			cache.Add (key, value);
		}

		/// <summary>
		/// Get the specified key from the cache.
		/// </summary>
		/// <param name="key">Key.</param>
		public V get(String key){
			V ret;
			cache.TryGetValue (key, out ret);
			return ret;
		}

		/// <summary>
		/// Gets the size of the cache
		/// </summary>
		/// <returns>The size.</returns>
		public int getSize(){
			return cache.Count;
		}

	}
}

