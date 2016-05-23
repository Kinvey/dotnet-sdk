using System;
using System.Collections.Generic;
namespace KinveyXamarin
{
	public class NetworkRequest <T> : AbstractKinveyClientRequest <T>
	{
		public NetworkRequest (AbstractClient client, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters) :
		base (client, client.BaseUrl, requestMethod, uriTemplate, httpContent, uriParameters)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClientRequest`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="requestMethod">Request method.</param>
		/// <param name="uriTemplate">URI template.</param>
		/// <param name="httpContent">Http content.</param>
		/// <param name="uriParameters">URI parameters.</param>
		public NetworkRequest(AbstractClient client, string baseURL, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters):
		base (client, baseURL, requestMethod, uriTemplate, httpContent, uriParameters)
		{}

	}
}

