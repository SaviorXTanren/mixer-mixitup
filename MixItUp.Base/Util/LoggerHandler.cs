using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Globalization;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class LoggerHandler
    {
        public static string CurrentLogFilePath { get; private set; }
        public static string CurrentChatEventLogFilePath { get; private set; }

        private const string LogsDirectoryName = "Logs";
        private const string LogFileNameFormat = "MixItUpLog-{0}.txt";

        private static IFileService fileService;

        public static void Initialize(IFileService fileService)
        {
            LoggerHandler.fileService = fileService;
            LoggerHandler.fileService.CreateDirectory(LogsDirectoryName);
            LoggerHandler.CurrentLogFilePath = Path.Combine(LogsDirectoryName, string.Format(LogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)));

            Logger.LogOccurred += Logger_LogOccurred;
        }

        private static void Logger_LogOccurred(object sender, Log log)
        {
            try
            {
                LoggerHandler.fileService.AppendFile(LoggerHandler.CurrentLogFilePath, string.Format("{0} - {1} - {2} " + Environment.NewLine + Environment.NewLine,
                    DateTimeOffset.Now.ToString(), EnumHelper.GetEnumName(log.Level), log.Message));
            }
            catch (Exception) { }
        }
    }
}
