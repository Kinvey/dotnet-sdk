using System;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Kinvey
{
    public static class Constants
    {
        public static readonly Uri DefaultApiUri = new Uri("https://baas.kinvey.com/");
        public static readonly Uri DefaultAuthUri = new Uri("https://auth.kinvey.com/");

        internal static readonly MediaTypeWithQualityHeaderValue MediaTypeJson = new MediaTypeWithQualityHeaderValue("application/json");
    }
}
