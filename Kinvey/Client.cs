using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey
{
    public class Client
    {

        public static readonly Uri DefaultApiUri = new Uri("https://baas.kinvey.com/");
        public static readonly Uri DefaultAuthUri = new Uri("https://auth.kinvey.com/");

        public static Client Shared = new Client();

        public string AppKey { get; private set; }
        public string AppSecret { get; private set; }
        public Uri ApiUri { get; private set; } = DefaultApiUri;
        public Uri AuthUri { get; private set; } = DefaultAuthUri;

        public HttpClient HttpClient = new HttpClient();

        public Client()
        {
        }

        public Client(string appKey, string appSecret, string instanceId)
        {
            Initialize(appKey, appSecret, instanceId);
        }

        public Client(string appKey, string appSecret, Uri apiUri = null, Uri authUri = null)
        {
            Initialize(appKey, appSecret, apiUri, authUri);
        }

        public void Initialize(string appKey, string appSecret, string instanceId)
        {
            Initialize(appKey, appSecret, new Uri($"https://{instanceId}-baas.kinvey.com/"), new Uri($"https://{instanceId}-auth.kinvey.com/"));
        }

        public void Initialize(string appKey, string appSecret, Uri apiUri = null, Uri authUri = null)
        {
            Validate(appKey, appSecret);

            AppKey = appKey;
            AppSecret = appSecret;
            ApiUri = apiUri ?? DefaultApiUri;
            AuthUri = authUri ?? DefaultAuthUri;
        }

        private void Validate(string appKey, string appSecret)
        {
            if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret))
            {
                throw new ArgumentException("Please provide a valid appKey and appSecret. Your app's key and secret can be found on the Kinvey management console.");
            }
        }

        private void Validate()
        {
            Validate(AppKey, AppSecret);
        }

        public async Task<EnvironmentInfo> PingAsync()
        {
            Validate();

            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ApiUri, $"/appdata/{AppKey}")))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{AppKey}:{AppSecret}")));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var response = await HttpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var serializer = new DataContractJsonSerializer(typeof(EnvironmentInfo));
                        var obj = serializer.ReadObject(stream) as EnvironmentInfo;
                        return obj;
                    }
                }
            }
        }
    }
}
