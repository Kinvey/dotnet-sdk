using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kinvey
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "_id", IsRequired = true)]
        public string userId;

        [DataMember(Name = "username", IsRequired = true)]
        public string Username;

        [DataMember(Name = "_kmd", IsRequired = true)]
        public UserMetadata Metadata;

        public AuthenticationHeaderValue AuthenticationHeaderValue => new AuthenticationHeaderValue("Kinvey", Metadata.Authtoken);

        public static async Task<User> SignupAsync(string username = default(string), string password = default(string), User user = default(User), Options? options = null, CancellationToken cancelationToken = default(CancellationToken))
        {
            return await SignupAsync<User>(username, password, user, options, cancelationToken);
        }

        public static async Task<U> SignupAsync<U>(string username = default(string), string password = default(string), U user = default(U), Options? options = null, CancellationToken cancelationToken = default(CancellationToken)) where U : User
        {
            var client = options?.Client ?? Client.SharedClient;
            client.ThrowIfNotInitialized();
            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(client.ApiUri, $"/user/{client.AppKey}")))
            {
                request.Headers.Authorization = client.AuthenticationHeaderValue;
                request.Headers.Accept.Add(Constants.MediaTypeJson);
                using (var response = await client.HttpClient.SendAsync(request, cancelationToken))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var userResult = client.JsonSerializer.FromJson(typeof(U), stream) as U;
                        client.ActiveUser = userResult as User;
                        return userResult;
                    }
                }
            }
        }
    }

    [DataContract]
    public class UserMetadata : Metadata
    {
        [DataMember(Name = "authtoken", IsRequired = true)]
        internal string Authtoken;
    }
}
