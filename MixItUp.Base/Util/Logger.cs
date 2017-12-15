using MixItUp.Base.Services;
using System;
using System.Globalization;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class Logger
    {
        private const string LogsDirectoryName = "Logs";
        private const string LogFileNameFormat = "MixItUpLog-{0}.txt";

        private static IFileService fileService;
        private static string CurrentLogFileName;

        public static void Initialize(IFileService fileService)
        {
            Logger.fileService = fileService;
            Logger.fileService.CreateDirectory(LogsDirectoryName);
            Logger.CurrentLogFileName = Path.Combine(LogsDirectoryName, string.Format(LogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));
        }

        public static void Log(string message)
        {
            try
            {
                Logger.fileService.SaveFile(Logger.CurrentLogFileName, message + Environment.NewLine + Environment.NewLine, create: false);
            }
            catch (Exception) { }
        }

        public static void Log(Exception ex) { Logger.Log(ex.ToString()); }
    }
}
