using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class FileSerializerHelper
    {
        private static IFileService fileService;

        public static void Initialize(IFileService fileService)
        {
            FileSerializerHelper.fileService = fileService;
        }

        public static async Task SerializeToFile<T>(string filePath, T data)
        {
            string dataString = JSONSerializerHelper.SerializeToString(data);
            if (!string.IsNullOrEmpty(dataString))
            {
                await FileSerializerHelper.fileService.SaveFile(filePath, dataString);
            }
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return JSONSerializerHelper.DeserializeFromString<T>(await FileSerializerHelper.fileService.ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }

        public static async Task<T> DeserializeAbstractFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return JSONSerializerHelper.DeserializeAbstractFromString<T>(await FileSerializerHelper.fileService.ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }
    }
}
