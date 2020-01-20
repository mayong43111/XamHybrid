using System;
using System.Threading;

namespace XHApp.Actions
{
    public interface IFaceCameraService
    {
        void OpenCameraAsync(Action<byte[]> pictureHandler, CancellationToken token = default(CancellationToken));
    }
}
