using MixItUp.Base.Services;
using System;
using System.Globalization;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class Logger
    {
        public static bool IsDebug { get; private set; }
        public static string CurrentLogFilePath { get; private set; }
        public static string CurrentChatEventLogFilePath { get; private set; }

        private const string LogsDirectoryName = "Logs";
        private const string LogFileNameFormat = "MixItUpLog-{0}.txt";

        private const string ChatEventLogDirectoryName = "ChatEventLogs";
        private const string ChatEventLogFileNameFormat = "ChatEventLog-{0}.txt";

        private static IFileService fileService;

        public static void Initialize(IFileService fileService)
        {
            Logger.fileService = fileService;
            Logger.fileService.CreateDirectory(LogsDirectoryName);
            Logger.CurrentLogFilePath = Path.Combine(LogsDirectoryName, string.Format(LogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));

            Logger.fileService.CreateDirectory(ChatEventLogDirectoryName);
            Logger.CurrentChatEventLogFilePath = Path.Combine(ChatEventLogDirectoryName, string.Format(ChatEventLogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));

            Mixer.Base.Util.Logger.LogOccurred += Logger_LogOccurred;

            #if DEBUG
                Logger.IsDebug = true;
            #endif
        }

        public static void Log(string message)
        {
            try
            {
                Logger.fileService.AppendFile(Logger.CurrentLogFilePath, DateTimeOffset.Now.ToString() + " - " + message + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception) { }
        }

        public static void Log(Exception ex, bool includeFullStackTrace = false, bool isCrashing = false)
        {
            string exString = ex.ToString();

            if (includeFullStackTrace)
            {
                exString += Environment.NewLine + "Full Stack:" + Environment.StackTrace;
            }

            Logger.Log(exString);
        }

        public static void LogDiagnostic(string message)
        {
            if (ChannelSession.Settings.DiagnosticLogging)
            {
                Logger.Log(message);
            }
        }

        public static void LogDiagnostic(Exception ex, bool includeFullStackTrace = false, bool isCrashing = false)
        {
            if (ChannelSession.Settings.DiagnosticLogging)
            {
                Logger.Log(ex, includeFullStackTrace, isCrashing);
            }
        }

        public static void LogChatEvent(string message)
        {
            if (ChannelSession.Settings.SaveChatEventLogs)
            {
                try
                {
                    Logger.fileService.AppendFile(Logger.CurrentChatEventLogFilePath, string.Format("{0} ({1}){2}", message, DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture), Environment.NewLine));
                }
                catch (Exception) { }
            }
        }

        private static void Logger_LogOccurred(object sender, string e)
        {
            Logger.Log(e);
        }
    }
}
