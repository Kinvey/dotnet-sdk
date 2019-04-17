using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.TestApp.Shared.Interfaces
{
    public interface IFCMService
    {
        Task RegisterAsync(Client client);

        Task UnRegisterAsync(Client client);
    }
}
