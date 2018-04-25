using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;

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

        [JsonIgnore]
        public bool SettingsChangeRestartRequired { get; set; }

        [DataMember]
        public bool InstallerFolderUpgradeAsked { get; set; }

        [DataMember]
        public uint AutoLogInAccount { get; set; }

        [DataMember]
        public string ThemeName { get; set; }
        [JsonIgnore]
        public bool IsDarkColoring { get { return App.AppSettings.ThemeName.Equals("Dark"); } }

        [DataMember]
        public string ColorScheme { get; set; }

        [DataMember]
        public string Language { get; set; }

        public ApplicationSettings()
        {
            this.AutoLogInAccount = 0;
            this.ThemeName = "Light";
            this.ColorScheme = "Indigo";
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
