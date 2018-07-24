using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kinvey.Json;

namespace Kinvey.Generic
{   
    public class Client<U> : ICredential where U : User
    {

        public string AppKey { get; private set; }
        public string AppSecret { get; private set; }
        public Uri ApiUri { get; private set; } = Constants.DefaultApiUri;
        public Uri AuthUri { get; private set; } = Constants.DefaultAuthUri;

        public AuthenticationHeaderValue AuthenticationHeaderValue => new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{AppKey}:{AppSecret}")));

        public HttpClient HttpClient = new HttpClient();
        public U ActiveUser { get; internal set; }
        public IJsonSerializer JsonSerializer = new DataContractJsonSerializer();

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
            ThrowIfInvalid(appKey, appSecret);

            AppKey = appKey;
            AppSecret = appSecret;
            ApiUri = apiUri ?? Constants.DefaultApiUri;
            AuthUri = authUri ?? Constants.DefaultAuthUri;
        }

        private void ThrowIfInvalid(string appKey, string appSecret)
        {
            if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret))
            {
                throw new ArgumentException("Please provide a valid appKey and appSecret. Your app's key and secret can be found on the Kinvey management console.");
            }
        }

        internal void ThrowIfNotInitialized()
        {
            ThrowIfInvalid(AppKey, AppSecret);
        }

        public async Task<EnvironmentInfo> PingAsync(CancellationToken cancelationToken = default(CancellationToken))
        {
            ThrowIfNotInitialized();

            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ApiUri, $"/appdata/{AppKey}")))
            {
                request.Headers.Authorization = AuthenticationHeaderValue;
                request.Headers.Accept.Add(Constants.MediaTypeJson);
                using (var response = await HttpClient.SendAsync(request, cancelationToken))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        return JsonSerializer.FromJson(typeof(EnvironmentInfo), stream) as EnvironmentInfo;
                    }
                }
            }
        }
    }
}
