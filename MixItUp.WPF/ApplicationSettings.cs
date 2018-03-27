using MixItUp.Base.Util;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.WPF
{
    [DataContract]
    public class ApplicationSettings
    {
        private const string ApplicationSettingsFileName = "ApplicationSettings.xml";

        public static ApplicationSettings Load()
        {
            if (File.Exists(ApplicationSettingsFileName))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(ApplicationSettingsFileName)))
                {
                    return SerializerHelper.DeserializeFromString<ApplicationSettings>(reader.ReadToEnd());
                }
            }
            return new ApplicationSettings();
        }

        [DataMember]
        public bool DarkTheme { get; set; }
        [DataMember]
        public string Language { get; set; }

        public ApplicationSettings()
        {
            this.DarkTheme = false;
            this.Language = "en";
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(File.Open(ApplicationSettingsFileName, FileMode.Create)))
            {
                writer.Write(SerializerHelper.SerializeToString(this));
                writer.Flush();
            }
        }
    }
}
