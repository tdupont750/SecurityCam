namespace SecurityCam.Configuration
{
    public class CameraConfig
    {
        public int PumpMs { get; set; } = 1000;
        public bool Render { get; set; } = true;
        public int Index { get; set; } = 0;
        public int Height { get; set; } = 1024;       
        public int Width { get; set; } = 1280;
        public int Fps { get; set; } = 2;
    }
}