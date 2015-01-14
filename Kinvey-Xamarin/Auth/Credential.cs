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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// This is a credential object, storing authentication information
	/// </summary>
	[DataContract]
	public class Credential : IKinveyRequestInitializer
	{
		/// <summary>
		/// The user _id.
		/// </summary>
		[DataMember]
		private string userId;
		/// <summary>
		/// The auth token.
		/// </summary>
		[DataMember]
		private string authToken;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Credential"/> class.
		/// </summary>
		internal Credential() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Credential"/> class.
		/// </summary>
		/// <param name="userId">User _id.</param>
		/// <param name="authToken">Auth token.</param>
		public Credential(string userId, string authToken)
		{
			this.userId = userId;
			this.authToken = authToken;
		}
		/// <summary>
		/// Gets or sets the user _id.
		/// </summary>
		/// <value>The user identifier.</value>
		public string UserId
		{
			get { return this.userId; }
			internal set { this.userId = value; }
		}

		/// <summary>
		/// Gets or sets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
		public string AuthToken
		{
			get { return this.authToken; }
			internal set { this.authToken = value; }
		}

		/// <summary>
		/// Initialize the specified clientRequest with this credential.
		/// </summary>
		/// <param name="clientRequest">Client Request.</param>
		/// <typeparam name="T">The type of the Client Request</typeparam>
		public void Initialize<T>(AbstractKinveyClientRequest<T> clientRequest)
		{
			if (authToken != null)
			{
				clientRequest.RequestAuth = new KinveyAuthenticator(authToken);
			}
		}

		/// <summary>
		/// Create a new Credential from a KinveyAuthResponse.
		/// </summary>
		/// <param name="response">The response of a Kinvey login/create request.</param>
		public static Credential From(KinveyAuthResponse response)
		{
			return new Credential(response.UserId, response.AuthToken);
		}

		/// <summary>
		/// Create a new Credential from a Kinvey User object.
		/// </summary>
		/// <param name="user">User.</param>
		public static Credential From(User user)
		{
			return new Credential(user.Id, user.AuthToken);
		}
	}
}

