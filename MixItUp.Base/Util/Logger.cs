using Mixer.Base.Web;
using MixItUp.Base.Services;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

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
                Logger.fileService.AppendFile(Logger.CurrentLogFileName, DateTimeOffset.Now.ToString() + " - " + message + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception) { }
        }

        public static void Log(Exception ex)
        {
            Logger.Log(ex.ToString());

            string exceptionJSON = JsonConvert.SerializeObject(ex.ToString());
            #if DEBUG
            #else
                Task.Run(async () => { await Logger.LogAnalyticsInternal("Exception", exceptionJSON); });
            #endif
        }

        public static async Task LogAnalyticsUsage(string eventName, string eventDetails)
        {
            #if DEBUG
                await Task.FromResult(0);
            #else
                await Logger.LogAnalyticsInternal(eventName, eventDetails);               
            #endif
        }

        private static async Task LogAnalyticsInternal(string eventName, string eventDetails)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper())
                {
                    client.BaseAddress = new Uri("https://api.mixitupapp.com/analytics/");
                    HttpResponseMessage response = await client.GetAsync(string.Format("log?username={0}&eventName={1}&eventDetails={2}&appVersion={3}",
                        (ChannelSession.User != null) ? ChannelSession.User.username : "UNKNOWN", eventName, eventDetails,
                        Assembly.GetEntryAssembly().GetName().Version.ToString()));
                }
            }
            catch (Exception) { }
        }
    }
}
