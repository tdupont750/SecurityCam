using System;

namespace SecurityCam.Configuration
{
    public class FileConfig
    {
        public string Dir { get; set; } = Environment.CurrentDirectory;
        public int Count { get; set; } = 100;
    }
}