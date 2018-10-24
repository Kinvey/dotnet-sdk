using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinvey.TestLocalLibApp.Interfaces;

namespace Kinvey.TestLocalLibApp.UWP
{
    public class WindowsAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Constants.Settings.AppKey, Constants.Settings.AppSecret);
        }
    }
}
