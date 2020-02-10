using MixItUp.Base;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Diagnostics;
using System.Globalization;
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
        private const string LogsDirectoryName = "Logs";
        private const string LogFileNameFormat = "MixItUpLog-{0}.txt";

        public static string CurrentLogFilePath { get; private set; }

        private bool crashObtained;

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.LogOccurred += Logger_LogOccurred;
            Directory.CreateDirectory(LogsDirectoryName);
            App.CurrentLogFilePath = Path.Combine(LogsDirectoryName, string.Format(LogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)));

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Logger.SetLogLevel(LogLevel.Information);
            Logger.Log(LogLevel.Information, "Auto Hoster Log");
            Logger.SetLogLevel(LogLevel.Error);

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

                ProcessHelper.LaunchProgram("MixItUp.Reporter.exe", string.Format("{0} {1}", (ChannelSession.MixerUser != null) ? ChannelSession.MixerUser.id : 0, FileLoggerHandler.CurrentLogFilePath));

                Task.Delay(1000).Wait();
            }
        }

        private static void Logger_LogOccurred(object sender, Log log)
        {
            try
            {
                File.AppendAllText(FileLoggerHandler.CurrentLogFilePath, string.Format("{0} - {1} - {2} " + Environment.NewLine + Environment.NewLine,
                    DateTimeOffset.Now.ToString(), EnumHelper.GetEnumName(log.Level), log.Message));
            }
            catch (Exception) { }
        }
    }
}
