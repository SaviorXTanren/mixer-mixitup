using MixItUp.Base.Services;
using Newtonsoft.Json;
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
            await SerializerHelper.fileService.SaveFile(filePath, SerializerHelper.SerializeToString(data));
        }

        public static string SerializeToString<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                return SerializerHelper.DeserializeFromString<T>(await SerializerHelper.fileService.ReadFile(filePath));
            }
            return default(T);
        }

        public static T DeserializeFromString<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public static T DeserializeAbstractFromString<T>(string data)
        {
            return (T)JsonConvert.DeserializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public static T Clone<T>(object data)
        {
            return SerializerHelper.DeserializeFromString<T>(SerializerHelper.SerializeToString(data));
        }
    }
}
