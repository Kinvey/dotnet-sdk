using System;
using Kinvey.DotNet.Framework;
using RestSharp;
using Kinvey.DotNet.Framework.Core;
using Kinvey.DotNet.Framework.Auth;

namespace KinveyXamarin
{
	public class Client : AbstractClient
	{
		protected Client(RestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
			: base(client, rootUrl, servicePath, initializer, store)
		{
		}
	}
}

