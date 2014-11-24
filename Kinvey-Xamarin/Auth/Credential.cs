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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	[DataContract]
	public class Credential : IKinveyRequestInitializer
	{
//		private static readonly string AuthHeaderFormat = "Kinvey %s";
		[DataMember]
		private string userId;
		[DataMember]
		private string authToken;

		internal Credential() { }

		public Credential(string userId, string authToken)
		{
			this.userId = userId;
			this.authToken = authToken;
		}

		public string UserId
		{
			get { return this.userId; }
			internal set { this.userId = value; }
		}

		public string AuthToken
		{
			get { return this.authToken; }
			internal set { this.authToken = value; }
		}

		public void Initialize<T>(AbstractKinveyClientRequest<T> clientRequest)
		{
			if (authToken != null)
			{
				clientRequest.RequestAuth = new KinveyAuthenticator(authToken);
			}
		}

		public static Credential From(KinveyAuthResponse response)
		{
			return new Credential(response.UserId, response.AuthToken);
		}

		public static Credential From(User user)
		{
			return new Credential(user.Id, user.AuthToken);
		}
	}
}

