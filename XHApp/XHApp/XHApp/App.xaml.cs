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

            AppCenter.Start("android=708f4271-7e68-43c4-8d27-fbe07e4e88f1;" +
                  "uwp={Your UWP App secret here};" +
                  "ios=d15c46b2-6708-40e1-8efe-478a44f9deb9",
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
