using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using SecurityCam.Configuration;

namespace SecurityCam.Services
{
    public class CameraService : IDisposable
    {
        private readonly CameraConfig _config;
        private readonly CancellationTokenSource _cancelSource;
        private readonly Window _window;
        private readonly VideoCapture _capture;

        public CameraService(CameraConfig config, CancellationTokenSource cancelSource)
        {
            _config = config;
            _cancelSource = cancelSource;
            
            _capture = VideoCapture.FromCamera(config.Index);
            _capture.Fps = config.Fps;
            _capture.FrameHeight = config.Height;
            _capture.FrameWidth = config.Width; 
            
            if (config.Render)
                _window = new Window("SecurityCam");
        }
        
        public void Dispose()
        {
            _window?.Dispose();
            _capture.Release();
            _capture.Dispose();
        }
        
        public void Delay(CancellationToken cancelToken)
        {
            if (_window == null)
            {
                Task.Delay(_config.PumpMs, cancelToken).Wait(cancelToken);
                return;
            }
            
            var key = Cv2.WaitKey(_config.PumpMs);

            if (key == (int) ConsoleKey.Escape)
                _cancelSource.Cancel();
        }

        public bool TryRead(Mat image)
        {
            if (!_capture.Read(image))
                return false;

            _window?.ShowImage(image);  
            return true;
        }

        public Scalar MeanStdDev(Mat image)
        {
            Cv2.MeanStdDev(image, out _, out var meanStdDev);
            return meanStdDev;
        }
    }
}