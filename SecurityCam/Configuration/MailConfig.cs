using System.Text.Json.Serialization;

namespace SecurityCam.Configuration
{
    public class MailConfig
    {
        public bool Enabled { get; set; } = false;
        public int Port { get; set; } = 587;
        public string From { get; set; } = "okiedokieobi@gmail.com";
        public string To { get; set; } = "grievous4x@gmail.com";
        public string Subject { get; set; } = "Hello there!";
        public string Body { get; set; } = "...automated email is so uncivilized.";
        public string Smtp { get; set; } = "smtp.gmail.com";
        [JsonIgnore] public string Password { get; set; } = "force4life";
    }
}