using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XHApp.Views;
using ZXing.Net.Mobile.Forms;

namespace XHApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));

            OpenTestCommand = new Command(async () =>
            {
                //await Shell.Current.Navigation.PushModalAsync(new TestPage());
                var scanPage = new ZXingScannerPage();
                scanPage.OnScanResult += (result) =>
                {
                    scanPage.IsScanning = false;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Shell.Current.Navigation.PopModalAsync();
                        Shell.Current.DisplayAlert("Message", result.Text, "OK");
                    });
                };

                await Shell.Current.Navigation.PushModalAsync(scanPage);
            });
        }

        public ICommand OpenWebCommand { get; }

        public ICommand OpenTestCommand { get; }
    }
}