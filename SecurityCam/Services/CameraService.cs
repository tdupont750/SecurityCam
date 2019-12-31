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
            
            if (config.Render)
                _window = new Window("SecurityCam");
            
            _capture = VideoCapture.FromCamera(config.Index);
            _capture.Fps = config.Fps;
            _capture.FrameHeight = config.Height;
            _capture.FrameWidth = config.Width;
        }
        
        public void Dispose()
        {
            _window?.Dispose();
            _capture.Dispose();
        }
        
        public Task DelayAsync(CancellationToken cancelToken)
        {
            if (_window == null)
                return Task.Delay(_config.PumpMs, cancelToken);
            
            var key = Cv2.WaitKey(_config.PumpMs);

            if (key == (int) ConsoleKey.Escape)
                _cancelSource.Cancel();

            return Task.CompletedTask;
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