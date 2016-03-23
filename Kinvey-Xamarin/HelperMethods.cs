using System;
using System.Linq;
using RestSharp;

namespace KinveyXamarin
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
			if (response.Headers != null)
			{
				var item = response.Headers
					.Cast<Parameter>()
					.SingleOrDefault(i => i.Name.Equals("X-Kinvey-Request-Id"))
					.Value;

				Type valueType = item.GetType();
				if ((valueType.IsArray) &&
					(valueType.GetElementType() == typeof(string)))
				{
					string[] arrRequestID = ((string[])item);
					if (arrRequestID != null &&
						arrRequestID.Length > 0)
					{
						return arrRequestID[0];
					}
				}
			}

			return null;
		}
	}
}

