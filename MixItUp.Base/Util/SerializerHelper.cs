using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class SerializerHelper
    {
        public static async Task SerializeToFile<T>(string filePath, T data)
        {
            using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
            {
                await writer.WriteAsync(SerializerHelper.SerializeToString(data));
            }
        }

        public static string SerializeToString<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static async Task<T> DeserializeFromFile<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    return SerializerHelper.DeserializeFromString<T>(await reader.ReadToEndAsync());
                }
            }
            return default(T);
        }

        public static T DeserializeFromString<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
