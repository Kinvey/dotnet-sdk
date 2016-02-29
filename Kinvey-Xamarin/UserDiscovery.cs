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

using System.Collections.Generic;

namespace KinveyXamarin
{
	/// <summary>
	/// The class used to define the parameters for a UserDiscovery lookup
	/// </summary>
	public class UserDiscovery
	{
		private string id;
		private string username;
		private string firstName;
		private string lastName;
		private string email;
		private string facebookID;

		/// <summary>
		/// The _id parameter used for a UserDiscovery lookup
		/// </summary>
		public string ID
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// The username parameter used for a UserDiscovery lookup
		/// </summary>
		public string Username
		{
			get { return this.username; }
			set { this.username = value; }
		}

		/// <summary>
		/// The first_name parameter used for a UserDiscovery lookup
		/// </summary>
		public string FirstName
		{
			get { return this.firstName; }
			set { this.firstName = value; }
		}

		/// <summary>
		/// The last_name parameter used for a UserDiscovery lookup
		/// </summary>
		public string LastName
		{
			get { return this.lastName; }
			set { this.lastName = value; }
		}

		/// <summary>
		/// The email parameter used for a UserDiscovery lookup
		/// </summary>
		public string Email
		{
			get { return this.email; }
			set { this.email = value; }
		}

		/// <summary>
		/// The _socialidentity.facebook.id parameter used for a UserDiscovery lookup
		/// </summary>
		public string FacebookID
		{
			get { return this.facebookID; }
			set { this.facebookID = value; }
		}

		/// <summary>
		/// Used to see the criteria set for a UserDiscovery lookup
		/// </summary>
		/// <returns>A dictionary of the criteria set up for UserDiscovery lookup</returns>
		public Dictionary<string, string> getCriteria()
		{
			Dictionary<string, string> criteria = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty (id))
			{
				criteria.Add ("_id", id);
			}
			if (!string.IsNullOrEmpty (username))
			{
				criteria.Add ("username", username);
			}
			if (!string.IsNullOrEmpty (firstName))
			{
				criteria.Add ("first_name", firstName);
			}
			if (!string.IsNullOrEmpty (lastName))
			{
				criteria.Add ("last_name", lastName);
			}
			if (!string.IsNullOrEmpty (email))
			{
				criteria.Add ("email", email);
			}
			if (!string.IsNullOrEmpty (facebookID))
			{
				criteria.Add ("_socialidentity.facebook.id", facebookID);
			}

			return criteria;
		}
	}
}
