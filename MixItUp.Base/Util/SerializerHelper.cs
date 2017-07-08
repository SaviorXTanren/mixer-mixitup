using System.IO;
using System.Xml.Serialization;

namespace MixItUp.Base.Util
{
    public static class SerializerHelper
    {
        public static string Serialize<T>(T data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }

        public static T Deserialize<T>(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
