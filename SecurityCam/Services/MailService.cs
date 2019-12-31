using System;
using System.IO;
using MailKit.Net.Smtp;
using MimeKit;
using SecurityCam.Configuration;
using SecurityCam.Diagnostics;

namespace SecurityCam.Services
{
    public class MailService : IDisposable
    {
        private readonly Lazy<SmtpClient> _smpt;
        private readonly MailConfig _config;
        private readonly ILog _log;

        public MailService(MailConfig config, ILog log)
        {
            _config = config;
            _log = log;

            _smpt = new Lazy<SmtpClient>(() =>
            {
                var client = new SmtpClient();
                client.Connect(config.Smtp, config.Port);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(config.From, config.Password);
                return client;
            });
        }

        public void Dispose()
        {
            if (!_smpt.IsValueCreated) return;
            _smpt.Value.Disconnect(true);
            _smpt.Value.Dispose();
        }

        public void Send(string fileName)
        {
            if (!_config.Enabled)
                return;
            
            _log.Write(LogLevel.Info, "Sending email");
            
            var body = new TextPart("plain")
            {
                Text = _config.Body
            };

            var attachment = new MimePart("image", "gif")
            {
                Content = new MimeContent(File.OpenRead(fileName)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(fileName)
            };

            var multipart = new Multipart("mixed") {body, attachment};
            
            var mime = new MimeMessage
            {
                To = { new MailboxAddress(_config.To)},
                From = { new MailboxAddress(_config.From)},
                Subject = _config.Subject,
                Body = multipart
            };

            _smpt.Value.Send(mime);   
        }
    }
}