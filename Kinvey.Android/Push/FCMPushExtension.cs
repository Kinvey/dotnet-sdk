namespace Kinvey
{
    public static class FCMPushExtension
    {
        public static FCMPush FcmPush(this Client client)
        {
            return new FCMPush(client);
        }
    }
}
