using System.Runtime.CompilerServices;

namespace SecurityCam.Diagnostics
{
    public interface ILog
    {
        public void Write(LogLevel level, string msg, [CallerFilePath] string filePath = default, [CallerMemberName] string caller = default, [CallerLineNumber] int line = default);
    }
}