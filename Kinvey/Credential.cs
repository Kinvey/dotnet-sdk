using System;
using System.Net.Http.Headers;

namespace Kinvey
{
    public interface ICredential
    {

        AuthenticationHeaderValue AuthenticationHeaderValue { get; }

    }
}
