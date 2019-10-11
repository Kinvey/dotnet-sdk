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

using Newtonsoft.Json;

namespace Kinvey
{
    /// <summary>
    /// This class represents the third party identity.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class ThirdPartyIdentity
	{
        /// <summary>
        /// The provider for the third party social identity.
        /// </summary>
        /// <value>The provider property gets/sets the value of the Provider field, _provider.</value>
		[JsonProperty("_socialIdentity")]
		public Provider provider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyIdentity"/> class.
        /// </summary>
        ///<param name="provider"> Provider. </param>
        public ThirdPartyIdentity (Provider provider)
		{
			this.provider = provider;
		}
	}

    /// <summary>
    /// This class represents the third party identity provider.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class Provider
	{
        /// <summary>
        /// The access token for Facebook.
        /// </summary>
        /// <value>The facebook property gets/sets the value of the AccessToken field, _facebook.</value>
		[JsonProperty("facebook")]
		public AccessToken facebook { get; set; }

        /// <summary>
        /// The access token for Google.
        /// </summary>
        /// <value>The google property gets/sets the value of the AccessToken field, _google.</value>
        [JsonProperty("google")]
		public AccessToken google { get; set; }

        /// <summary>
        /// The access token for Twitter.
        /// </summary>
        /// <value>The twitter property gets/sets the value of the AccessToken field, _twitter.</value>
        [JsonProperty("twitter")]
		public AccessToken twitter { get; set; }

        /// <summary>
        /// The access token for Linkedin.
        /// </summary>
        /// <value>The linkedin property gets/sets the value of the AccessToken field, _linkedin.</value>
        [JsonProperty("linkedin")]
		public AccessToken linkedin { get; set; }

        /// <summary>
        /// The access token for authentication link.
        /// </summary>
        /// <value>The authlink property gets/sets the value of the AccessToken field, _authlink.</value>
        [JsonProperty("authlink")]
		public AccessToken authlink { get; set; }

        /// <summary>
        /// The access token for salesforce.
        /// </summary>
        /// <value>The salesforce property gets/sets the value of the AccessToken field, _salesforce.</value>
        [JsonProperty("salesforce")]
		public AccessToken salesforce { get; set; }

        /// <summary>
        /// The access token for Kinvey authentication.
        /// </summary>
        /// <value>The kinveyAuth property gets/sets the value of the AccessToken field, _kinveyAuth.</value>
        [JsonProperty("kinveyAuth")]
		public AccessToken kinveyAuth { get; set; }
	}

    /// <summary>
    /// This class represents Facebook credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class FacebookCredential : OAuth2
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookCredential"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
        public FacebookCredential(string accessToken) : base(accessToken) {}
	}

    /// <summary>
    /// This class represents Google credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class GoogleCredential : OAuth2
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCredential"/> class.
        /// </summary>
        /// <param name="accesstoken"> Access token. </param>
        public GoogleCredential(string accesstoken) : base(accesstoken) {}
	}

    /// <summary>
    /// This class represents Twitter credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class TwitterCredential : OAuth1
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterCredential"/> class.
        /// </summary>
        /// <param name="accesstoken"> Access token. </param>
        /// <param name="accesstokensecret"> Access token secret. </param>
        /// <param name="consumerkey"> Consumer key. </param>
        /// <param name="consumersecret"> Consumer secret. </param>
        public TwitterCredential(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret) : base(accesstoken, accesstokensecret, consumerkey, consumersecret) {}
	}

    /// <summary>
    /// This class represents LinkedIn credential.
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class LinkedInCredential : OAuth1
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedInCredential"/> class.
        /// </summary>
        /// <param name="accesstoken"> Access token. </param>
        /// <param name="accesstokensecret"> Access token secret. </param>
        /// <param name="consumerkey"> Consumer key. </param>
        /// <param name="consumersecret"> Consumer secret. </param>
        public LinkedInCredential(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret) : base(accesstoken, accesstokensecret, consumerkey, consumersecret) {}
	}

    /// <summary>
    /// This class represents authentication link credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class AuthLinkCredential : OAuth2
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthLinkCredential"/> class.
        /// </summary>
        /// <param name="accesstoken"> Access token. </param>
        /// <param name="refreshtoken"> Refresh token. </param>
		public AuthLinkCredential(string accesstoken, string refreshtoken) : base(accesstoken, refreshtoken) {}
	}

    /// <summary>
    /// This class represents salesforce credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class SalesforceCredential : OAuth2
	{
        /// <summary>
        /// Client identifier.
        /// </summary>
        /// <value>The client_id property gets/sets the value of the string field, _client_id.</value>
        [JsonProperty]
		public string client_id { get; set; }

        /// <summary>
        /// Identifier.
        /// </summary>
        /// <value>The id property gets/sets the value of the string field, _id.</value>
        [JsonProperty]
		public string id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceCredential"/> class.
        /// </summary>
        /// <param name="access"> Access token. </param>
        /// <param name="reauth"> Refresh token. </param>
        /// <param name="clientid"> Client id. </param>
        /// <param name="id"> Identifier. </param>
        public SalesforceCredential(string access, string reauth, string clientid, string id) : base (access, reauth)
		{
			this.client_id = clientid;
			this.id = id;
		}
	}

    /// <summary>
    /// This class represents MIC credential.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class MICCredential : OAuth2
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="MICCredential"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
        public MICCredential(string accessToken) : base(accessToken) {}
	}

    /// <summary>
    /// This class represents credential for OAuth2.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class OAuth2 : AccessToken
	{
		[JsonProperty("refresh_token")]
		private string refreshToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
		public OAuth2(string accessToken) : base(accessToken) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
        /// <param name="refreshtoken"> Refresh token. </param>
        public OAuth2(string accessToken, string refreshtoken) : base(accessToken)
		{
			this.refreshToken = refreshtoken;
		}
	}

    /// <summary>
    /// This class represents credential for OAuth1.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class OAuth1 : AccessToken
	{
        /// <summary>
        /// Access token secret.
        /// </summary>
        /// <value>The accessTokenSecret property gets/sets the value of the string field, _accessTokenSecret.</value>
		[JsonProperty("access_token_secret")]
		protected string accessTokenSecret { get; set; }

        /// <summary>
        /// Consumer key.
        /// </summary>
        /// <value>The consumerKey property gets/sets the value of the string field, _consumerKey.</value>
        [JsonProperty("consumer_key")]
		protected string consumerKey { get; set; }

        /// <summary>
        /// Consumer secret.
        /// </summary>
        /// <value>The consumerSecret property gets/sets the value of the string field, _consumerSecret.</value>
        [JsonProperty("consumer_secret")]
		protected string consumerSecret { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth1"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
        public OAuth1(string accessToken) : base(accessToken) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth1"/> class.
        /// </summary>
        /// <param name="accessToken"> Access token. </param>
        /// <param name="accesstokensecret"> Access token secret. </param>
        /// <param name="consumerkey"> Consumer key. </param>
        /// <param name="consumersecret"> Consumer secret. </param>
        public OAuth1(string accessToken, string accesstokensecret, string consumerkey, string consumersecret) : base(accessToken)
		{
			this.accessTokenSecret = accesstokensecret;
			this.consumerKey = consumerkey;
			this.consumerSecret = consumersecret;
		}
	}

    /// <summary>
    /// This class represents access token.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class AccessToken
	{
        /// <summary>
        /// Access token.
        /// </summary>
        /// <value>The accessToken property gets/sets the value of the string field, _accessToken.</value>
		[JsonProperty("access_token")]
		public string accessToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessToken"/> class.
        /// </summary>
        /// <param name="access"> Access token. </param>
        public AccessToken(string access)
		{
			this.accessToken = access;
		}
	}
}
