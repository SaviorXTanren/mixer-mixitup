using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class FileSerializerHelper
    {
        public static async Task SerializeToFile<T>(string filePath, T data)
        {
            string dataString = JSONSerializerHelper.SerializeToString(data);
            if (!string.IsNullOrEmpty(dataString))
            {
                await ServiceManager.Get<IFileService>().SaveFile(filePath, dataString);
            }
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return JSONSerializerHelper.DeserializeFromString<T>(await ServiceManager.Get<IFileService>().ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }

        public static async Task<T> DeserializeAbstractFromFile<T>(string filePath, bool ignoreErrors = false)
        {
            if (File.Exists(filePath))
            {
                return JSONSerializerHelper.DeserializeAbstractFromString<T>(await ServiceManager.Get<IFileService>().ReadFile(filePath), ignoreErrors);
            }
            return default(T);
        }
    }
}
