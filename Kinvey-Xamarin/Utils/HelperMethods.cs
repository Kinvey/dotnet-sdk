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
using System.Linq;
using RestSharp;

namespace Kinvey
{
	public class HelperMethods
	{
		/// <summary>
		/// Gets the Request ID associated with the given response.
		/// </summary>
		/// <returns>The request ID.</returns>
		/// <param name="response">Response object.</param>
		public static string getRequestID(IRestResponse response)
		{
			if (response != null && response.Headers != null)
			{
				try
				{
					var item = response.Headers
						.Cast<Parameter>()
						.SingleOrDefault(i => i.Name.ToLower().Equals ("x-kinvey-request-id"))
						.Value;

					Type valueType = item.GetType();
					if (valueType != null)
					{
						if (valueType.IsArray)
						{
							if (valueType.GetElementType() == typeof(string))
							{
								string[] arrRequestID = ((string[])item);
								if (arrRequestID != null &&
									arrRequestID.Length > 0)
								{
									return arrRequestID[0];
								}
							}
						}
						else if (valueType.IsConstructedGenericType)
						{
							if (valueType.Name.Contains("List"))
							{
								var listRequestID = ((System.Collections.Generic.List<string>)item);
								if (listRequestID != null &&
								    listRequestID.Count > 0)
								{
									return listRequestID.First();
								}
							}
						}
					}
				}
				catch(Exception e)
				{
					return string.Empty;
				}
			}

			return string.Empty;
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
	}
}
