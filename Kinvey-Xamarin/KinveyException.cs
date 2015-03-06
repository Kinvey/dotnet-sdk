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
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// Wrapper for a kinvey specific exception containing information about how to resolve the issue. 
	/// </summary>
    public class KinveyException : Exception 
    {
		/// <summary>
		/// The reason.
		/// </summary>
        private string reason;
		/// <summary>
		/// The fix.
		/// </summary>
        private string fix;
		/// <summary>
		/// The explanation.
		/// </summary>
        private string explanation;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyException"/> class.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
        public KinveyException(string reason, string fix, string explanation)
            : base(FormatMessage(reason, fix, explanation))
        {
            this.reason = reason;
            this.fix = fix;
            this.explanation = explanation;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyException"/> class.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
		public KinveyException(string reason)
			: base(FormatMessage(reason))
		{
			this.reason = reason;
		}
			
		/// <summary>
		/// Gets or sets the reason.
		/// </summary>
		/// <value>The reason.</value>
        public string Reason
        {
            get { return reason; }
            set { this.reason = value; }
        }

      
		/// <summary>
		/// Gets or sets the fix.
		/// </summary>
		/// <value>The fix.</value>
        public string Fix
        {
            get { return fix; }
            set { this.fix = value; }
        }

		/// <summary>
		/// Gets or sets the explanation.
		/// </summary>
		/// <value>The explanation.</value>
        public string Explanation
        {
            get { return explanation; }
            set { this.explanation = value; }
        }

		/// <summary>
		/// Formats the message.
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
        private static String FormatMessage(string reason, string fix, string explanation)
        {
            return "\nREASON: " + reason + "\n" + "FIX: " + fix + "\n" + "EXPLANATION: " + explanation + "\n";
        }

		/// <summary>
		/// Formats the message.
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="reason">Reason.</param>
		private static String FormatMessage(string reason)
		{
			return "\nREASON: " + reason;
		}

    }
}
