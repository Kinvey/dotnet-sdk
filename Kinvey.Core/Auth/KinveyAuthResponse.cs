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

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// This class represents the response of a Kinvey Auth Request.
	/// </summary>
	[JsonObject]
	public class KinveyAuthResponse : JObject
    {

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthResponse"/> class.
		/// </summary>
        public KinveyAuthResponse() { }

		/// <summary>
		/// Gets or sets the user identifier.
		/// </summary>
		/// <value>The user identifier.</value>
        [JsonProperty("_id")]
        public string UserId { get; set; }

		/// <summary>
		/// Gets or sets the user metadata.
		/// </summary>
		/// <value>The user metadata.</value>
        [JsonProperty("_kmd")]
        public KinveyUserMetaData UserMetaData { get; set; }

		/// <summary>
		/// Gets or sets the auth social identity.
		/// </summary>
		/// <value>The auth social identity.</value>
		[JsonProperty("_socialIdentity")]
		public KinveyAuthSocialID AuthSocialIdentity { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The username.</value>
		[JsonProperty("username")]
		public string username { get; set; }

		/// <summary>
		/// Gets or sets the attributes.
		/// </summary>
		/// <value>The attributes.</value>
		[JsonExtensionData]
		public Dictionary<string, JToken> Attributes { get; set; }

		/// <summary>
		/// Gets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
        public string AuthToken
        {
            get { return (UserMetaData != null ? UserMetaData.AuthToken : null); }
        }

		/// <summary>
		/// Gets the access token.
		/// </summary>
		/// <value>The access token.</value>
		public string AccessToken
		{
			get { return AuthSocialIdentity?.AuthMetaData?.AccessToken; }
		}
    }
}
