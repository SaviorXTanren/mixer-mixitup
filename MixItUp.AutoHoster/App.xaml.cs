using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Desktop.Files;
using MixItUp.Desktop.Services;
using StreamingClient.Base.Util;
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
        public static IFileService FileService { get; private set; }

        private bool crashObtained;

        protected override void OnStartup(StartupEventArgs e)
        {
            App.FileService = new WindowsFileService();
            FileLoggerHandler.Initialize(App.FileService);
            SerializerHelper.Initialize(App.FileService);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Logger.Log("Auto Hoster Application Version: " + App.FileService.GetApplicationVersion());

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

                Logger.Log("CRASH OCCURRED");
                Logger.Log(ex, includeStackTrace: true);

                string reporterFilePath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), "MixItUp.Reporter.exe");
                ProcessStartInfo processStartInfo = new ProcessStartInfo(reporterFilePath, string.Format("{0} {1}", (ChannelSession.MixerStreamerUser != null) ? ChannelSession.MixerStreamerUser.id : 0, FileLoggerHandler.CurrentLogFilePath));
                Process.Start(processStartInfo);

                Task.Delay(1000).Wait();
            }
        }
    }
}
