using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    public class KinveyUnitTest
    {

        protected static readonly string AppKey = Environment.GetEnvironmentVariable("KINVEY_APP_KEY");
        protected static readonly string AppSecret = Environment.GetEnvironmentVariable("KINVEY_APP_SECRET");
        protected static readonly string EnvironmentInfoAppName = Environment.GetEnvironmentVariable("KINVEY_ENVINFO_APPNAME");
        protected static readonly string EnvironmentInfoVersion = Environment.GetEnvironmentVariable("KINVEY_ENVINFO_VERSION");
        protected static readonly string EnvironmentInfoKinvey = Environment.GetEnvironmentVariable("KINVEY_ENVINFO_KINVEY");
        protected static readonly string EnvironmentInfoEnvironmentName = Environment.GetEnvironmentVariable("KINVEY_ENVINFO_ENVNAME");

        protected static readonly bool UseMock = string.IsNullOrEmpty(AppKey) && string.IsNullOrEmpty(AppSecret);

        protected class MockHttpClientHandler : HttpClientHandler
        {
            public delegate HttpResponseMessage SendDelegate(HttpRequestMessage request);
            public event SendDelegate Send;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Send.Invoke(request));
            }
        }

        protected HttpResponseMessage MockResponse<T>(T obj, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            string expectedResponse;
            using (var memoryStream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(memoryStream, obj);
                expectedResponse = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(expectedResponse)
            };
            return response;
        }

        protected HttpClient MockHttpClient(MockHttpClientHandler.SendDelegate sendDelegate)
        {
            var mockHandler = new MockHttpClientHandler();
            mockHandler.Send += sendDelegate;
            return new HttpClient(mockHandler);
        }

    }
}
