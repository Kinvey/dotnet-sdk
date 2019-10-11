// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kinvey
{
    /// <summary>
    /// Base class for pushes.
    /// </summary>
    public abstract class AbstractPush
	{
        /// <summary>
        /// Client that the user is logged in. 
        /// </summary>
        /// <value>The Kinvey client.</value>
        protected Client client;

        /// <summary>
        /// The constant representing the string with <c>android</c> value.
        /// </summary>
        /// <value>The string with <c>android</c> value.</value>
        protected const string ANDROID = "android";

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractPush"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        public AbstractPush (Client client)
		{
			this.client = client;
		}

        /// <summary>
        /// Enables pushes.
        /// </summary>
        /// <param name="platform"> Platform. </param>
        /// <param name="deviceId"> Device identifier. </param>
        /// <returns> The asynchronous task with push data. </returns>
        public async Task<PushPayload> EnablePushAsync(string platform, string deviceId)
        {
			return await EnablePushViaRest (platform, deviceId).ExecuteAsync ();
		}

        /// <summary>
        /// Disables pushes.
        /// </summary>
        /// <param name="platform"> Platform. </param>
        /// <param name="deviceId"> Device identifier. </param>
        /// <returns> The asynchronous task with push data. </returns>
		public async Task<PushPayload> DisablePushAsync(string platform, string deviceId)
        {
			return await DisablePushViaRest (platform, deviceId).ExecuteAsync ();
		}

        /// <summary>
        /// Creates a request for enabling pushes.
        /// </summary>
        /// <param name="platform"> Platform. </param>
        /// <param name="deviceId"> Device identifier. </param>
        /// <returns> The request for enabling pushes. </returns>
        public EnablePush EnablePushViaRest(string platform, string deviceId)
        {
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (platform, deviceId);


			EnablePush enable = new EnablePush (client, input, urlParameters);

			client.InitializeRequest(enable);

			return enable;
		}

        /// <summary>
        /// Creates a request for disabling pushes.
        /// </summary>
        /// <param name="platform"> Platform. </param>
        /// <param name="deviceId"> Device identifier. </param>
        /// <returns> The request for disabling pushes. </returns>
        public RemovePush DisablePushViaRest(string platform, string deviceId)
        {
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (platform, deviceId);

			RemovePush disable = new RemovePush (client, input, urlParameters);

			client.InitializeRequest(disable);

			return disable;
		}

        /// <summary>
        /// This class represents the request for enabling pushes.
        /// </summary>
        public class EnablePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/register-device";

            /// <summary>
            /// Initializes a new instance of the <see cref="EnablePush"/> class.
            /// </summary>
            /// <param name="client"> Client that the user is logged in. </param>
            /// <param name="input"> Information about push. </param>
            /// <param name="urlProperties"> Url properties. </param>
            public EnablePush(AbstractClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}

        /// <summary>
        /// This class represents the request for disabling pushes.
        /// </summary>
        public class RemovePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/unregister-device";

            /// <summary>
            /// Initializes a new instance of the <see cref="RemovePush"/> class.
            /// </summary>
            /// <param name="client"> Client that the user is logged in. </param>
            /// <param name="input"> Information about push. </param>
            /// <param name="urlProperties"> Url properties. </param>
			public RemovePush(AbstractClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}

        /// <summary>
        /// This class represents push payload.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
		public class PushPayload : JObject{

			[JsonProperty]
			private string platform { get; set;} 

			[JsonProperty]
			private string deviceId { get; set;}

            [JsonProperty]
            private string service { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PushPayload"/> class.
            /// </summary>
            /// <param name="platform"> Platform. </param>
            /// <param name="deviceId"> Device identifier. </param>
            public PushPayload(string platform, string deviceId) {
				this.platform = platform;
				this.deviceId = deviceId;

                if (platform.Equals(ANDROID))
                {
                    this.service = "firebase";
                }
            }
		}
	}
}

