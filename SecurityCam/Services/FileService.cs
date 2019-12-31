using System;
using System.IO;
using System.Linq;
using OpenCvSharp;
using SecurityCam.Configuration;

namespace SecurityCam.Services
{
    public class FileService
    {
        private readonly FileConfig _config;

        public FileService(FileConfig config)
        {
            _config = config;
        }
        
        public string Write(Mat image)
        {
            var filePath = Path.Combine(_config.Dir, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.png");
            image.SaveImage(filePath);
            CleanOldFiles();
            return filePath;
        }
        
        private void CleanOldFiles()
        {
            var files = Directory
                .GetFiles(_config.Dir)
                .OrderBy(f => f)
                .ToArray();
            
            foreach(var file in files.Skip(_config.Count))
                File.Delete(file);
            
        }
    }
}