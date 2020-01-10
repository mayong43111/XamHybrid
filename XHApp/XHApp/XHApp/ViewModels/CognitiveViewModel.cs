using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XHApp.Actions;
using XHApp.Models;

namespace XHApp.ViewModels
{
    public class CognitiveViewModel : BaseViewModel
    {
        public ICommand TakePhotoCommand { get; protected set; }
        public ICommand PickPhotoCommand { get; protected set; }
        public ICommand EditFaceCommand { get; protected set; }

        private ObservableCollection<FaceClientResultModel> _faces = new ObservableCollection<FaceClientResultModel>();

        public ObservableCollection<FaceClientResultModel> Faces
        {
            get => _faces;
            set => SetProperty(ref _faces, value, "Faces");
        }

        private ImageSource _currentImage;

        public ImageSource CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value, "CurrentImage");
        }

        public CognitiveViewModel()
        {
            Title = "人脸识别";
            TakePhotoCommand = new Command(async () => await TakePhoto());
            PickPhotoCommand = new Command(async () => await PickPhoto());
            EditFaceCommand = new Command<FaceClientResultModel>(async model => await EditFace(model));
        }

        private async Task TakePhoto()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera))[Permission.Camera];
                }

                if (status == PermissionStatus.Granted)
                {
                    await CrossMedia.Current.Initialize();

                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        return;
                    }

                    var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { SaveMetaData = true });

                    await Detection(file);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }
        }

        private async Task PickPhoto()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera))[Permission.Camera];
                }

                if (status == PermissionStatus.Granted)
                {
                    await CrossMedia.Current.Initialize();

                    if (!CrossMedia.Current.IsPickVideoSupported)
                    {
                        return;
                    }

                    var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions());
                    await Detection(file);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }
        }

        private async Task Detection(MediaFile file)
        {
            if (file == null)
                return;

            IsBusy = true;

            CurrentImage = file.Path;

            try
            {
                this.Faces = await FaceClientWrapperr.Instace.Detection(file);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }

            IsBusy = false;
        }

        private async Task EditFace(FaceClientResultModel model)
        {
            var result = await Shell.Current.DisplayPromptAsync("姓名", model.Name);

            if (!string.IsNullOrWhiteSpace(result))
            {
                IsBusy = true;

                try
                {
                    model.Name = result;
                    await FaceClientWrapperr.Instace.Training(model);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
                }

                IsBusy = false;
            }
        }
    }
}
