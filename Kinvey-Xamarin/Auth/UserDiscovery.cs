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
		/// <summary>
		/// The _id parameter used for a UserDiscovery lookup
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// The username parameter used for a UserDiscovery lookup
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// The first_name parameter used for a UserDiscovery lookup
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// The last_name parameter used for a UserDiscovery lookup
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// The email parameter used for a UserDiscovery lookup
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// The _socialidentity.facebook.id parameter used for a UserDiscovery lookup
		/// </summary>
		public string FacebookID { get; set; }

		/// <summary>
		/// Used to see the criteria set for a UserDiscovery lookup
		/// </summary>
		/// <returns>A dictionary of the criteria set up for UserDiscovery lookup</returns>
		public Dictionary<string, string> getCriteria()
		{
			Dictionary<string, string> criteria = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty (ID))
			{
				criteria.Add ("_id", ID);
			}
			if (!string.IsNullOrEmpty (Username))
			{
				criteria.Add ("username", Username);
			}
			if (!string.IsNullOrEmpty (FirstName))
			{
				criteria.Add ("first_name", FirstName);
			}
			if (!string.IsNullOrEmpty (LastName))
			{
				criteria.Add ("last_name", LastName);
			}
			if (!string.IsNullOrEmpty (Email))
			{
				criteria.Add ("email", Email);
			}
			if (!string.IsNullOrEmpty (FacebookID))
			{
				criteria.Add ("_socialidentity.facebook.id", FacebookID);
			}

			return criteria;
		}
	}
}
