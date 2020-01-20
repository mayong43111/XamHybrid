using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;
using System.Threading;
using Xamarin.Forms;
using XHApp.Actions;
using XHApp.Droid.CustomControls;
using XHApp.Droid.Services;

[assembly: Dependency(typeof(FaceCameraService))]

namespace XHApp.Droid.Services
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class FaceCameraService : IFaceCameraService
    {
        internal static event EventHandler CancelRequested;

        private int requestId;
        private readonly Context context;

        public bool IsCameraAvailable { get; }

        public FaceCameraService()
        {
            this.context = Android.App.Application.Context;
            IsCameraAvailable = context.PackageManager.HasSystemFeature(PackageManager.FeatureCamera);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Gingerbread)
                IsCameraAvailable |= context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraFront);
        }

        public void OpenCameraAsync(Action<byte[]> pictureHandler, CancellationToken token = default(CancellationToken))
        {
            if (!IsCameraAvailable)
                throw new NotSupportedException();

            var id = GetRequestId();

            void handler(object s, OnPictureTakenEventArgs e)
            {
                FaceCameraActivity.OnPictureTaken -= handler;

                if (e.RequestId != id)
                    return;

                if (!e.IsCanceled && e.Error == null)
                {
                    pictureHandler(e.Data);
                }
            }

            token.Register(() =>
            {
                FaceCameraActivity.OnPictureTaken -= handler;

                CancelRequested?.Invoke(null, EventArgs.Empty);
                CancelRequested = null;
            }, true);

            FaceCameraActivity.OnPictureTaken += handler;

            context.StartActivity(CreateMediaIntent(id));
        }

        private int GetRequestId()
        {
            var id = requestId;
            if (requestId == int.MaxValue)
                requestId = 0;
            else
                requestId++;

            return id;
        }

        private Intent CreateMediaIntent(int id)
        {
            var pickerIntent = new Intent(this.context, typeof(FaceCameraActivity));
            pickerIntent.PutExtra(FaceCameraActivity.ExtraId, id);
            pickerIntent.SetFlags(ActivityFlags.NewTask);

            return pickerIntent;
        }
    }
}