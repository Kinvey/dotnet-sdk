using System;
using System.Collections.Generic;
using System.Text;

namespace Kinvey.Tests
{
    public class FakeRequest : AbstractKinveyClientRequest<ToDo>
    {
        private const string REST_PATH = "fake_request";

        public FakeRequest(AbstractClient client, Dictionary<string, string> urlProperties) :
        base(client, "POST", REST_PATH, null, urlProperties)
        {

        }
    }
}
