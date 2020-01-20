using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XHApp.Actions;

namespace XHApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public ICommand VisionCommand { get; protected set; }

        private ImageSource _currentImage;

        public ImageSource CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value, "CurrentImage");
        }

        private string _userNames;

        public string UserNames
        {
            get => _userNames;
            set => SetProperty(ref _userNames, value, "UserNames");
        }

        public LoginViewModel()
        {
            Title = "扫脸登录";
            VisionCommand = new Command(() => TakePhoto());
        }

        private void TakePhoto()
        {
            if (CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera).GetAwaiter().GetResult() != PermissionStatus.Granted)
            {
                CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
            }

            if (CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera).GetAwaiter().GetResult() == PermissionStatus.Granted)
            {
                var tokenSource = new CancellationTokenSource();

                DependencyService.Get<IFaceCameraService>()
                    .OpenCameraAsync(data =>
                    {
                        this.Detection(data);
                        tokenSource.Cancel();
                    }, tokenSource.Token);
            }
        }

        private void Detection(byte[] data)
        {
            try
            {
                ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                this.CurrentImage = ImageSource.FromStream(() => new MemoryStream(data));
                this.UserNames = "计算中...";

                Task.Run(async () =>
                {
                    var faces = await FaceClientWrapperr.Instace.Detection(data);
                    this.UserNames = string.Join(" | ", faces.Where(f => !string.IsNullOrEmpty(f.Name)).Select(f => f.Name));
                });
            }
            catch (Exception ex)
            {
                this.UserNames = ex.Message;
            }
        }
    }
}
