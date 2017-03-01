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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kinvey
{
	public abstract class AbstractPush
	{

		protected Client client;


		public AbstractPush (Client client)
		{
			this.client = client;
		}

		public async Task<PushPayload> EnablePushAsync(string platform, string entityId){
			return await EnablePushViaRest (platform, entityId).ExecuteAsync ();
		}

		public async Task<PushPayload> DisablePushAsync(string platform, string entityId){
			return await DisablePushViaRest (platform, entityId).ExecuteAsync ();
		}

		public EnablePush EnablePushViaRest(string platform, string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (platform, deviceId);


			EnablePush enable = new EnablePush (client, input, urlParameters);

			client.InitializeRequest(enable);

			return enable;
		}

		public RemovePush DisablePushViaRest(string platform, string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (platform, deviceId);

			RemovePush disable = new RemovePush (client, input, urlParameters);

			client.InitializeRequest(disable);

			return disable;
		}


		public class EnablePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/register-device";

			public EnablePush(AbstractClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}

		public class RemovePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/unregister-device";

			public RemovePush(AbstractClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}


		[JsonObject(MemberSerialization.OptIn)]
		public class PushPayload : JObject{

			[JsonProperty]
			private String platform { get; set;} 

			[JsonProperty]
			private String deviceId {get; set;}

			public PushPayload(string platform, string deviceId) {
				this.platform = platform;
				this.deviceId = deviceId;
			}
		}
	}
}

