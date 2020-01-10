using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XHApp.Services;
using XHApp.Views;

namespace XHApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<MockToolsDataStore>();

            MainPage = new AppShell();

            AppCenter.Start("0339f275-e252-4120-8001-b619d998ad86",
                               typeof(Analytics), typeof(Crashes));
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
