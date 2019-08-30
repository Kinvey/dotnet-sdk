// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using Newtonsoft.Json;

namespace Kinvey
{
	public abstract class AbstractDataRequest<T> : AbstractKinveyClientRequest<T>{
		[JsonProperty]
		public string CollectionName { get; set; }

		public AbstractDataRequest (AbstractClient client, string method, string template, Object httpContent, string collection): base(client, method, template, httpContent, new Dictionary<string, string>()){
			this.CollectionName = collection;
			uriResourceParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			uriResourceParameters.Add("collectionName", collection);
		}
	}}

