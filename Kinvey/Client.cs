using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kinvey.Tests")]
namespace Kinvey
{
    public class Client : Generic.Client<User>
    {
        public static Client SharedClient = new Client();

        public Client() : base()
        {
        }

        public Client(string appKey, string appSecret, string instanceId) : base(appKey, appSecret, instanceId)
        {
        }

        public Client(string appKey, string appSecret, Uri apiUri = null, Uri authUri = null) : base(appKey, appSecret, apiUri, authUri)
        {
        }
    }
}
