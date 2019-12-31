using System;
using System.IO;
using System.Runtime.CompilerServices;
using SecurityCam.Configuration;

namespace SecurityCam.Diagnostics.Implementation
{
    public class ConsoleLog : ILog
    {
        private readonly LogConfig _config;

        public ConsoleLog(LogConfig config)
        {
            _config = config;
        }
        
        public void Write(LogLevel level, string msg, [CallerFilePath] string filePath = default, [CallerMemberName] string caller = default, [CallerLineNumber] int line = default)
        {
            if (level < _config.MinLevel)
                return;
            
            var levelStr =  level switch
            {
                LogLevel.Debug => "DBG",
                LogLevel.Info => "INF",
                LogLevel.Error => "ERR",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
            
            Console.WriteLine($"[{levelStr}] {DateTime.Now:HH:mm:ss} - {Path.GetFileName(filePath)}.{caller}:{line} - {msg}");
        }
    }
}