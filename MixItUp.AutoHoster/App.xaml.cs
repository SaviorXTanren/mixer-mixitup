using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Desktop.Files;
using MixItUp.Desktop.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MixItUp.AutoHoster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool crashObtained;

        protected override void OnStartup(StartupEventArgs e)
        {
            WindowsFileService fileService = new WindowsFileService();
            Logger.Initialize(fileService);
            SerializerHelper.Initialize(fileService);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Logger.Log("Auto Hoster Application Version: " + fileService.GetApplicationVersion());

            base.OnStartup(e);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) { this.HandleCrash(e.Exception); }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) { this.HandleCrash((Exception)e.ExceptionObject); }

        private void HandleCrash(Exception ex)
        {
            if (!this.crashObtained)
            {
#if DEBUG
                Debugger.Break();
#endif

                this.crashObtained = true;

                if (ChannelSession.Services.Telemetry != null)
                {
                    ChannelSession.Services.Telemetry.TrackException(ex);
                }

                try
                {
                    using (StreamWriter writer = File.AppendText(Logger.CurrentLogFilePath))
                    {
                        writer.WriteLine("CRASHING EXCEPTION: " + Environment.NewLine + ex.ToString());
                    }
                }
                catch (Exception) { }
                Logger.Log(ex, includeFullStackTrace: false, isCrashing: true);

                string reporterFilePath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), "MixItUp.Reporter.exe");
                ProcessStartInfo processStartInfo = new ProcessStartInfo(reporterFilePath, string.Format("{0} {1}", (ChannelSession.User != null) ? ChannelSession.User.id : 0, Logger.CurrentLogFilePath));
                Process.Start(processStartInfo);

                Task.Delay(1000).Wait();
            }
        }
    }
}
