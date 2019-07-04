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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Kinvey
{
	public class HelperMethods
	{
		/// <summary>
		/// Gets the Request ID associated with the given response.
		/// </summary>
		/// <returns>The request ID.</returns>
		/// <param name="response">Response object.</param>
		public static string getRequestID(HttpResponseMessage response)
		{
			if (response != null && response.Headers != null)
			{
				try
				{
                    var keyValue = response.Headers
                                           .SingleOrDefault(i => i.Key.ToLower().Equals("x-kinvey-request-id"));

                    if (keyValue.Value == null) return string.Empty;
                    var item = keyValue.Value;
                    return item.First();
				}
				catch(Exception)
				{
					return string.Empty;
				}
			}

			return string.Empty;
		}

        public static string GetRequestStartTime(HttpResponseMessage response)
        {
            string XKinveyRequestStart = string.Empty;

            if (response != null && response.Headers != null)
            {
                try
                {
                    var keyValuePair = response.Headers
                                       .SingleOrDefault(i => i.Key.ToLower().Equals(Constants.STR_HEADER_REQUEST_START_TIME));
                    if (keyValuePair.Value == null) return string.Empty;
                    var item = keyValuePair.Value;
                    var requestStart = item.FirstOrDefault();
                    if (requestStart != null) XKinveyRequestStart = requestStart;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            return XKinveyRequestStart;
        }

		internal static bool IsDateMoreRecent(string checkDate, string origDate)
		{
			// First check if strings are equal, to potentially avoid
			// expensive date object parsing and comparison
			if (String.Compare(checkDate, origDate) == 0)
			{
				return false;
			}

			if (CompareDates(checkDate, origDate) > 0)
			{
				return true;
			}

			return false;
		}

		internal static int CompareDates(string date1, string date2)
		{
			// Returns 1 if date1 is more recent than date2
			// Returns 0 if both dates are equal
			// Returns -1 if date1 is less recent than date2
			DateTime dateToCheck = DateTime.Parse(date1);
			DateTime dateOfOrig = DateTime.Parse(date2);
			return dateToCheck.CompareTo(dateOfOrig);
		}

        internal static List<T> Initialize<T>(T value, int count)
        {
            var list = new List<T>(count);
            list.AddRange(Enumerable.Repeat(value, count));
            return list;
        }

        internal static bool IsLessThan(string checkingValue, int comparingValue)
        {
            return !int.TryParse(checkingValue, out int checkingValueNumber) || checkingValueNumber < comparingValue;
        }
    }
}
