using System;
using Android.App;
using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(GCMService))]

namespace Kinvey.TestLocalLibApp.Droid
{
    [Service]
    public class GCMService : KinveyGCMService, IGCMService
    {
        public event EventHandler changed;

        public override void onMessage(String message)
        {
            InvokeEvent(message);
        }
        public override void onError(String error)
        {
            InvokeEvent(error);
        }
        public override void onDelete(int deleted)
        {
            InvokeEvent(deleted.ToString());
        }
        public override void onRegistered(String gcmID)
        {
            InvokeEvent(gcmID);
        }
        public override void onUnregistered(String oldID)
        {
            InvokeEvent(oldID);
        }
        private void InvokeEvent(string message)
        {
            //changed(this, new GCMEventArgs(message));
        }

        public void Register(Client client)
        {
            client.Push().Initialize(Android.App.Application.Context);
        }

        public void Disable(Client client)
        {
            client.Push().DisablePush(Android.App.Application.Context);
        }
    }
}