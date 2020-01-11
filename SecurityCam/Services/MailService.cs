using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SecurityCam.Configuration;
using SecurityCam.Diagnostics;

namespace SecurityCam.Services
{
    public class MailService : IDisposable
    {
        private readonly MailConfig _config;
        private readonly ILog _log;
        
        private SmtpClient _smtpClient;
        private Task _previousSend = Task.CompletedTask;
        private int _sendCount;

        public MailService(MailConfig config, ILog log)
        {
            _config = config;
            _log = log;
        }

        public void Dispose()
        {
            _previousSend.Wait();
            
            EnsureDisposeSmtpClient();
        }

        public void Send(string fileName, CancellationToken cancelToken)
        {
            if (!_config.Enabled)
                return;

            var sendCount = Interlocked.Increment(ref _sendCount);
            _log.Write(LogLevel.Info, $"Send Start - SendCount: {sendCount}");
            
            // Don't leave the main thread, it will mess up the camera window renderer
            _previousSend.Wait(cancelToken);
            
            var mime = CreateMimeMessage(fileName);
            _previousSend = Task.Run(() => SendAsync(mime, sendCount, cancelToken), cancelToken);
        }

        private async Task SendAsync(MimeMessage mime, int sendCount, CancellationToken cancelToken)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                await EnsureCreateSmtpClientAsync(cancelToken);
                await _smtpClient.SendAsync(mime, cancelToken);
                _log.Write(LogLevel.Info, $"Send Complete - SendCount: {sendCount} - Elapsed: {sw.ElapsedMilliseconds}ms");
                return;
            }
            catch (TaskCanceledException) when (cancelToken.IsCancellationRequested)
            {
                _log.Write(LogLevel.Debug, $"Send Cancelled - SendCount: {sendCount} - Elapsed: {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Error, $"Send Failure - SendCount: {sendCount} - Elapsed: {sw.ElapsedMilliseconds}ms - Ex: {ex}");
            }
            finally
            {
                sw.Stop();
            }

            // Exited due to exception, dispose SmtpClient
            EnsureDisposeSmtpClient();
        }

        private MimeMessage CreateMimeMessage(string fileName)
        {
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
            
            return new MimeMessage
            {
                To = { new MailboxAddress(_config.To)},
                From = { new MailboxAddress(_config.From)},
                Subject = _config.Subject,
                Body = multipart
            };
        }

        private async Task EnsureCreateSmtpClientAsync(CancellationToken cancelToken)
        {
            if (_smtpClient != null)
                return;

            _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_config.Smtp, _config.Port, SecureSocketOptions.StartTls, cancelToken);

            _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            await _smtpClient.AuthenticateAsync(_config.From, _config.Password, cancelToken);
        }
        
        private void EnsureDisposeSmtpClient()
        {
            if (_smtpClient == null)
                return;
            
            try
            {
                _smtpClient.Dispose();
                _smtpClient = null;
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Error, $"SMTP Dispose Failure - Ex: {ex}");
            }
        }
    }
}