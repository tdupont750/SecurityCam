namespace SecurityCam.Configuration
{
    public class Config
    {
        public MailConfig Mail { get; } = new MailConfig();
        public FileConfig Files { get; } = new FileConfig();
        public TriggerConfig Trigger { get; } = new TriggerConfig();
        public CameraConfig Camera { get; } = new CameraConfig();
        public LogConfig Log { get; } = new LogConfig();
    }
}