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

namespace Kinvey
{
    /// <summary>	
    /// Instructs a linker to preserve the decorated code
    /// </summary>
	public sealed class PreserveAttribute : System.Attribute {
        /// <summary>	
        /// Ensures that all members of this type are preserved.
        /// </summary>
        /// <value> If the value is set <c>true</c> all members of this type, including fields, properties, methods, subclasses are preserved during linking. </value>
        public bool AllMembers;

        /// <summary>	
        /// Flags the method as a method to preserve during linking if the container class is pulled in.
        /// </summary>
        /// <value> If the value is set <c>true</c> a <see cref="PreserveAttribute"/> attribute on a method, then the method will be preserved. </value>
		public bool Conditional;
	}
}
