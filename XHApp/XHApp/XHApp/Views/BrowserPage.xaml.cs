using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.DeviceInfo.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Media.Abstractions;
using ZXing.Net.Mobile.Forms;

namespace XHApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowserPage : ContentPage
    {
        public BrowserPage()
        {
            InitializeComponent();
            SetWebviewCallbackAndEvents();
        }

        private void SetWebviewCallbackAndEvents()
        {
            try
            {

                //This will be triggered by webview
                appWebView.AddLocalCallback("camera", (str) =>
                {
                    //DisplayAlert(str, "拍照", "OK");
                    Task.Run(async () =>
                        {
                            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
                            {
                                await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
                                await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                            }

                            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
                            {
                                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                                {
                                    Directory = "Sample",
                                    Name = "test.jpg"
                                });
                            }
                            else
                            {
                                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                                //On iOS you may want to send your user to the settings screen.
                                //CrossPermissions.Current.OpenAppSettings();
                            }
                        });
                });

                appWebView.AddLocalCallback("qrcode", (str) =>
                {
                    //DisplayAlert(str, "扫码", "OK");

                    Device.BeginInvokeOnMainThread(async () =>
                        {
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
                });

                appWebView.AddLocalCallback("viber", (str) =>
                {
                    //DisplayAlert(str, "震动", "OK");

                    Plugin.Vibrate.CrossVibrate.Current.Vibration(TimeSpan.FromSeconds(1));
                });

                appWebView.AddLocalCallback("device", (str) =>
                {
                    //DisplayAlert(str, "取设备信息", "OK");

                    var deviceInfo = Plugin.DeviceInfo.CrossDeviceInfo.Current;
                    DisplayAlert("取设备信息", $"{deviceInfo.Manufacturer}-{deviceInfo.Model}-{deviceInfo.DeviceName}", "OK");
                });
            }
            catch { }
        }

        protected override void OnDisappearing()
        {
            this.appWebView.Dispose();
            base.OnDisappearing();
        }
    }
}