
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XHApp.Actions;
using XHApp.ViewModels;

namespace XHApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExternalPage : ContentPage
    {
        private ExternalViewModel viewModel;

        public ExternalPage(ExternalViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartPage(viewModel.Uri);
        }

        internal void StartPage(string uri)
        {
            UriBuilder source = new UriBuilder(uri);
            source.Query += string.IsNullOrWhiteSpace(source.Query) ? string.Empty : "&";

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    source.Query += "origin=ios-xinfangxiang";
                    break;
                case Device.Android:
                    source.Query += "origin=android-xinfangxiang";
                    break;
                default:
                    break;
            }

            this.appWebView.Source = source.ToString();
        }
    }
}