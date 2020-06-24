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

using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// Kinvey json error.
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class KinveyJsonError
    {
        /// <summary>
        /// Gets or sets the request ID that generated this JSON error.
        /// </summary>
        /// <value>The Kinvey request ID.</value>
        [JsonProperty]
		public string RequestID { get; set; }

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>The error.</value>
        [JsonProperty]
        public string Error {get; set;}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
        [JsonProperty]
        public string Description {get; set;}

		/// <summary>
		/// Gets or sets the debug message of the error.
		/// </summary>
		/// <value>The debug.</value>
        [JsonProperty]
        public JToken Debug {get; set;}

        /// <summary>
        /// Parses the specified response into an error.
        /// </summary>
        /// <param name="response">Http response.</param>
        /// <returns>Kinvey json error.</returns>
        public static KinveyJsonError parse(HttpResponseMessage response)
        {
            var task = response.Content.ReadAsStringAsync();
            task.Wait();
            return JsonConvert.DeserializeObject<KinveyJsonError>(task.Result);
        }
    }
}
