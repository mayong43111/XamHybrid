using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Vision;
using Android.Gms.Vision.Faces;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XHApp.Droid.Services;
using static Android.Gms.Vision.MultiProcessor;

namespace XHApp.Droid.CustomControls
{
    [Activity(Label = "FaceCameraActivity")]
    public class FaceCameraActivity : Activity, IFactory
    {
        internal const string ExtraId = "id";

        internal const string TAG = "FaceTracker";
        internal const int RC_HANDLE_GMS = 9001;
        // permission request codes need to be < 256
        internal const int RC_HANDLE_CAMERA_PERM = 2;

        internal static event EventHandler<OnPictureTakenEventArgs> OnPictureTaken;

        private int id;

        private CameraSource mCameraSource = null;
        private CameraSourcePreview mPreview;
        private GraphicOverlay mGraphicOverlay;

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean("ran", true);
            outState.PutInt(ExtraId, id);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FaceCameraService.CancelRequested += CancellationRequested;

            var b = (savedInstanceState ?? Intent.Extras);
            id = b.GetInt(ExtraId, 0);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.FaceCamera);

            mPreview = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            mGraphicOverlay = FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
            {
                CreateCameraSource();
            }
            else { RequestCameraPermission(); }
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mPreview.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mCameraSource != null)
            {
                mCameraSource.Release();
            }
        }

        private void RequestCameraPermission()
        {
            Log.Warn(TAG, "Camera permission is not granted. Requesting permission");

            var permissions = new string[] { Manifest.Permission.Camera };

            if (!ActivityCompat.ShouldShowRequestPermissionRationale(this,
                    Manifest.Permission.Camera))
            {
                ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM);
                return;
            }

            Snackbar.Make(mGraphicOverlay, "permission_camera_rationale",
                    Snackbar.LengthIndefinite)
                    .SetAction("ok", (o) => { ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM); })
                    .Show();
        }

        /**
         * Creates and starts the camera.  Note that this uses a higher resolution in comparison
         * to other detection examples to enable the barcode detector to detect small barcodes
         * at long distances.
         */
        private void CreateCameraSource()
        {

            var context = Application.Context;
            FaceDetector detector = new FaceDetector.Builder(context)
                    .SetClassificationType(ClassificationType.All)
                    .Build();

            detector.SetProcessor(
                    new MultiProcessor.Builder(this)
                            .Build());

            if (!detector.IsOperational)
            {
                // Note: The first time that an app using face API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any faces.
                //
                // isOperational() can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
                Log.Warn(TAG, "Face detector dependencies are not yet available.");
            }

            mCameraSource = new CameraSource.Builder(context, detector)
                    .SetRequestedPreviewSize(640, 480)
                                            .SetFacing(CameraFacing.Front)
                    .SetRequestedFps(30.0f)
                    .Build();


        }

        /**
         * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
         * (e.g., because onResume was called before the camera source was created), this will be called
         * again when the camera source is created.
         */
        private void StartCameraSource()
        {

            // check that the device has play services available.
            int code = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(
                    this.ApplicationContext);
            if (code != ConnectionResult.Success)
            {
                Dialog dlg =
                        GoogleApiAvailability.Instance.GetErrorDialog(this, code, RC_HANDLE_GMS);
                dlg.Show();
            }

            if (mCameraSource != null)
            {
                try
                {
                    mPreview.Start(mCameraSource, mGraphicOverlay);
                }
                catch (System.Exception e)
                {
                    Log.Error(TAG, "Unable to start camera source.", e);
                    mCameraSource.Release();
                    mCameraSource = null;
                }
            }
        }
        public Tracker Create(Java.Lang.Object item)
        {
            return new GraphicFaceTracker(id, OnPictureTaken, mGraphicOverlay, mCameraSource);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != RC_HANDLE_CAMERA_PERM)
            {
                Log.Debug(TAG, "Got unexpected permission result: " + requestCode);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                return;
            }

            if (grantResults.Length != 0 && grantResults[0] == Permission.Granted)
            {
                Log.Debug(TAG, "Camera permission granted - initialize the camera source");
                // we have permission, so create the camerasource
                CreateCameraSource();
                return;
            }

            Log.Error(TAG, "Permission not granted: results len = " + grantResults.Length +
                    " Result code = " + (grantResults.Length > 0 ? grantResults[0].ToString() : "(empty)"));


            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("LiveCam")
                    .SetMessage("no_camera_permission")
                    .SetPositiveButton("ok", (o, e) => Finish())
                    .Show();

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            FaceCameraService.CancelRequested -= CancellationRequested;
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void CancellationRequested(object sender, EventArgs e)
        {
            FinishActivity(id);
            Finish();
        }
    }

    class GraphicFaceTracker : Tracker, CameraSource.IPictureCallback
    {
        private int id;
        private EventHandler<OnPictureTakenEventArgs> onPictureTaken;
        private GraphicOverlay mOverlay;
        private FaceGraphic mFaceGraphic;
        private CameraSource mCameraSource = null;
        private bool isProcessing = false;

        public GraphicFaceTracker(int id, EventHandler<OnPictureTakenEventArgs> onPictureTaken, GraphicOverlay overlay, CameraSource cameraSource = null)
        {
            this.id = id;
            mOverlay = overlay;
            mFaceGraphic = new FaceGraphic(overlay);
            mCameraSource = cameraSource;
            this.onPictureTaken = onPictureTaken;
        }

        public override void OnNewItem(int id, Java.Lang.Object item)
        {
            mFaceGraphic.SetId(id);
        }

        public override void OnUpdate(Detector.Detections detections, Java.Lang.Object item)
        {
            var face = item as Face;
            mOverlay.Add(mFaceGraphic);
            mFaceGraphic.UpdateFace(face);

            try
            {
                if (mCameraSource != null && !isProcessing && face.Width > 250 && face.Height > 250 && face.Position.X > 50 && face.Position.Y > 150 && face.Position.X < 350 && face.Position.Y < 450)
                    mCameraSource.TakePicture(null, this);
            }
            catch { }
        }

        public override void OnMissing(Detector.Detections detections)
        {
            mOverlay.Remove(mFaceGraphic);

        }

        public override void OnDone()
        {
            mOverlay.Remove(mFaceGraphic);

        }

        public void OnPictureTaken(byte[] data)
        {
            isProcessing = true;

            Task.Run(() =>
            {
                try
                {
                    onPictureTaken?.Invoke(null, new OnPictureTakenEventArgs(id, false, data));
                }
                finally
                {
                    isProcessing = false;
                }
            });
        }
    }

    internal class OnPictureTakenEventArgs
        : EventArgs
    {
        public OnPictureTakenEventArgs(int id, Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            RequestId = id;
            Error = error;
        }

        public OnPictureTakenEventArgs(int id, bool isCanceled, byte[] data = null)
        {
            RequestId = id;
            IsCanceled = isCanceled;
            if (!IsCanceled && data == null)
                throw new ArgumentNullException("media");

            Data = data;
        }

        public int RequestId
        {
            get;
            private set;
        }

        public bool IsCanceled
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get;
            private set;
        }
    }
}