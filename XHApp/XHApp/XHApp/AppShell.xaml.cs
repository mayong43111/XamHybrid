using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XHApp.Views;

namespace XHApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("test", typeof(TestPage));
        }
    }
}
