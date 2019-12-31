using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenCvSharp;
using SecurityCam.Configuration;
using SecurityCam.Diagnostics;
using SecurityCam.Diagnostics.Implementation;
using SecurityCam.Services;

namespace SecurityCam
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {            
            var cancelSource = new CancellationTokenSource();
            Console.CancelKeyPress += (o, e) => cancelSource.Cancel();

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
                .Bind<Config>();

            var log = new ConsoleLog(config.Log);
            log.Write(LogLevel.Info, JsonSerializer.Serialize(config, new JsonSerializerOptions {WriteIndented = true}));

            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    await RunAsync(config, log, cancelSource);
                }
                catch (TaskCanceledException) when (cancelSource.IsCancellationRequested)
                {
                    // Ignore and exit
                }
                catch (Exception ex)
                {
                    log.Write(LogLevel.Error, ex.ToString());
                    await Task.Delay(10000, cancelSource.Token);
                }
            }
        }

        private static async Task RunAsync(Config config, ILog log, CancellationTokenSource cancelSource)
        {
            var trigger = new TriggerService(log, config.Trigger);
            var files = new FileService(config.Files);

            using var mail = new MailService(config.Mail, log);
            using var camera = new CameraService(config.Camera, cancelSource);
            using var image = new Mat();
            
            if (!camera.TryRead(image))
                return;
            
            var oldDev = camera.MeanStdDev(image);
                
            while (!cancelSource.IsCancellationRequested)
            {
                await camera.DelayAsync(cancelSource.Token);

                if (!camera.TryRead(image))
                    break;

                var newDev = camera.MeanStdDev(image);
                var diff = oldDev.Diff(newDev);
                    
                if (trigger.ShouldTrigger(diff))
                {
                    var path = files.Write(image);
                    mail.Send(path);
                }

                oldDev = newDev;
            }
        }
    }
}