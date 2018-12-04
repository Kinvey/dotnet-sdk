using System;
using System.Collections.Generic;
using System.Text;

namespace Kinvey.TestApp.Shared.Interfaces
{
    public interface IFCMService
    {
        void Register(Client client);

        void UnRegister(Client client);
    }
}
