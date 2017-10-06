using System;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class Logger
    {
        private const string LogFileName = "DataLog.txt";

        public static void Initialize()
        {
            using (FileStream stream = File.Open(LogFileName, FileMode.Create)) { }
        }

        public static void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(File.Open(LogFileName, FileMode.Append)))
            {
                writer.WriteLine(message);
            }
        }

        public static void Log(Exception ex) { Logger.Log(ex.ToString()); }
    }
}
