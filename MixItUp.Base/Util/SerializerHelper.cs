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
            string dataString = JSONSerializerHelper.SerializeToString(data);
            if (!string.IsNullOrEmpty(dataString))
            {
                await SerializerHelper.fileService.SaveFile(filePath, dataString);
            }
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return JSONSerializerHelper.DeserializeFromString<T>(await SerializerHelper.fileService.ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }

        private static void IgnoreDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            Logger.Log(errorArgs.ErrorContext.Error.Message);
            errorArgs.ErrorContext.Handled = true;
        }
    }
}
