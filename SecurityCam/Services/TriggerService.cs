using System;
using SecurityCam.Configuration;
using SecurityCam.Diagnostics;

namespace SecurityCam.Services
{
    public class TriggerService
    {
        private readonly ILog _log;
        private readonly TriggerConfig _config;

        private int _streak;
        private int _requirement;
        
        public TriggerService(ILog log, TriggerConfig config)
        {
            _log = log;
            _config = config;
            _requirement = config.MinReq;
        }
        
        public bool ShouldTrigger(double diff)
        {
            if (diff > _config.Threshold)
            {
                _log.Write(LogLevel.Debug, $"Over Threshold - Diff: {diff} - Threshold: {_config.Threshold}");
                
                if (++_streak < _requirement) 
                    return false;
                
                
                _log.Write(LogLevel.Info, $"Triggered - Streak: {_streak} - Requirement: {_requirement}");
                
                _streak = 0;
                _requirement++;
                
                return true;
            }

            _streak = 0;
            if (_requirement > _config.MinReq) _requirement--;
            return false;
        }
    }
}