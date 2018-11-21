using System;

namespace Kinvey.Kinvey.TestApp.Shared.Interfaces
{
    public interface IGCMService
    {
        event EventHandler changed;
        void Register(Client client);
        void Disable(Client client);

    }

    public class GCMEventArgs : EventArgs
    {
        public string data { get; set; }
        public GCMEventArgs(string data)
        {
            this.data = data;
        }
    }
}