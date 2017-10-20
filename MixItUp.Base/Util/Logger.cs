using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class Logger
    {
        private const string LogsDirectoryName = "Logs";
        private const string LogFileNameFormat = "MixItUpLog-{0}.txt";

        private static string CurrentLogFileName;

        public static void Initialize()
        {
            if (!Directory.Exists(LogsDirectoryName))
            {
                Directory.CreateDirectory(LogsDirectoryName);
            }
            Logger.CurrentLogFileName = Path.Combine(LogsDirectoryName, string.Format(LogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));
        }

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(File.Open(Logger.CurrentLogFileName, FileMode.Append)))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception) { }
        }

        public static void Log(Exception ex) { Logger.Log(ex.ToString()); }
    }
}
