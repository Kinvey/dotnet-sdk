using System;
using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestApp.Shared.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Kinvey.TestNugetPackageLibApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new ContractsPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            var builder = DependencyService.Get<IBuilder>().GetBuilder();
            builder.Build();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
