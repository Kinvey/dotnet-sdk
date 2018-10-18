using System;

namespace Kinvey
{
    public class AndroidClientBuilder : Client.Builder
    {
        public AndroidClientBuilder(string appKey, string appSecret) :
        base(appKey, appSecret, Constants.DevicePlatform.Android)
        {
            this.setFilePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
        }
    }
}
