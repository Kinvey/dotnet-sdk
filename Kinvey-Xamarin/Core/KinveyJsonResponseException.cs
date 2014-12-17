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
using System.IO;
using RestSharp;
using KinveyXamarin;
using KinveyUtils;

namespace KinveyXamarin
{
	/// <summary>
	/// An exeception based on a Kinvey JSON response.
	/// </summary>
    public class KinveyJsonResponseException : Exception
    {
		/// <summary>
		/// The error this expection wraps.
		/// </summary>
        private readonly KinveyJsonError details;
		/// <summary>
		/// The message.
		/// </summary>
        private string message;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyJsonResponseException"/> class.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="details">Details.</param>
		/// <param name="message">Message.</param>
		private KinveyJsonResponseException(IRestResponse response, KinveyJsonError details, string message) : base(response.ErrorMessage, response.ErrorException)
        {
            this.details = details;
            this.message = message;
        }

		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <value>The message.</value>
        public override string Message
        {
            get { return this.message; }
        }

		/// <summary>
		/// Gets the details.
		/// </summary>
		/// <value>The details.</value>
        public KinveyJsonError Details
        {
            get { return this.details; }
        }

		/// <summary>
		/// Creates a Response Execption from a RestRequest response.
		/// </summary>
		/// <param name="response">Response.</param>
		public static KinveyJsonResponseException From(IRestResponse response)
        {
            KinveyJsonError details = null;
            try
            {
				if (response.ErrorException != null){
					//it was a client side error
					details = new KinveyJsonError();
					details.Error = response.ErrorException.Message;
					details.Description = response.ErrorException.Source;
					details.Debug = response.ErrorException.StackTrace;
				}else if (response.Content != null){
					details = KinveyJsonError.parse(response);
				}
            }
            catch (IOException ex)
            {
				Logger.Log (ex.Message);

            } 
            
			string detailMessage = (details == null ? "unknown" : details.Error + " /"  + details.Description);
            return new KinveyJsonResponseException(response, details, detailMessage);
        }
    }
}
