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
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.DotNet.Framework
{
    public class KinveyException : Exception 
    {

        private string reason;
        private string fix;
        private string explanation;

        public KinveyException(string reason, string fix, string explanation)
            : base(FormatMessage(reason, fix, explanation))
        {
            this.reason = reason;
            this.fix = fix;
            this.explanation = explanation;
        }


        public string Reason
        {
            get { return reason; }
            set { this.reason = value; }
        }

      
        public string Fix
        {
            get { return fix; }
            set { this.fix = value; }
        }

        public string Explanation
        {
            get { return explanation; }
            set { this.explanation = value; }
        }

        private static String FormatMessage(string reason, string fix, string explanation)
        {
            return "\nREASON: " + reason + "\n" + "FIX: " + fix + "\n" + "EXPLANATION: " + explanation + "\n";
        }

    }
}
