

namespace Kinvey
{
    public class DotnetClientBuilder : Client.Builder
    {
        public DotnetClientBuilder(string appKey, string appSecret) :
        base(appKey, appSecret, Constants.DevicePlatform.NET)
        {
        }
    }
}
