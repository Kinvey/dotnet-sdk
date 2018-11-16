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

namespace Kinvey
{
	/// <summary>
	/// Persistable interface which model objects can choose to implement as an alternative
	/// to subclassing from <see cref="Entity"/>
	/// </summary>
	public interface IPersistable
	{
		/// <summary>
		/// ID field which maps back to Kinvey _id
		/// </summary>
		/// <value>The identifier.</value>
		string ID { get; set; }

		/// <summary>
		/// <see cref="AccessControlList"/>  field which maps back to Kinvey _acl
		/// </summary>
		/// <value>The acl.</value>
		AccessControlList Acl { get; set; }

		/// <summary>
		/// <see cref="KinveyMetaData"/> field which maps back to Kinvey _kmd
		/// </summary>
		/// <value>The kmd.</value>
		KinveyMetaData Kmd { get; set; }

        [Obsolete("This property has been deprecated. Please use Acl instead.")]
        AccessControlList ACL { get; set; }

        [Obsolete("This property has been deprecated. Please use Kmd instead.")]
        KinveyMetaData KMD { get; set; }
    }
}
