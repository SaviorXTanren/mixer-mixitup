using MixItUp.Base.Services;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class SerializerHelper
    {
        private static IFileService fileService;

        public static void Initialize(IFileService fileService)
        {
            SerializerHelper.fileService = fileService;
        }

        public static async Task SerializeToFile<T>(string filePath, T data)
        {
            string dataString = SerializerHelper.SerializeToString(data);
            if (!string.IsNullOrEmpty(dataString))
            {
                await SerializerHelper.fileService.SaveFile(filePath, dataString);
            }
        }

        public static string SerializeToString<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return SerializerHelper.DeserializeFromString<T>(await SerializerHelper.fileService.ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }

        public static T DeserializeFromString<T>(string data, bool ignoreErrors = false)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            };

            if (ignoreErrors)
            {
                serializerSettings.Error = IgnoreDeserializationError;
            }

            return JsonConvert.DeserializeObject<T>(data, serializerSettings);
        }

        public static T Clone<T>(object data)
        {
            return SerializerHelper.DeserializeFromString<T>(SerializerHelper.SerializeToString(data));
        }

        private static void IgnoreDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            Logger.Log(errorArgs.ErrorContext.Error.Message);
            errorArgs.ErrorContext.Handled = true;
        }
    }
}
