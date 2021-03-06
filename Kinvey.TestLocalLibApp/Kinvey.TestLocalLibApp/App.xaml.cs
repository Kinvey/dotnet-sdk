﻿using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestApp.Shared.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Kinvey.TestLocalLibApp
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
            //Creating and building of client.
            var builder = DependencyService.Get<IBuilder>().GetBuilder();
            builder.SetProjectId("637762827614").Build();
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
