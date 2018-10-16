using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
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
            ApplicationSettings settings = new ApplicationSettings();
            if (File.Exists(ApplicationSettingsFileName))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(ApplicationSettingsFileName)))
                {
                    settings = SerializerHelper.DeserializeFromString<ApplicationSettings>(reader.ReadToEnd());
                    if (settings == null)
                    {
                        settings = new ApplicationSettings();
                    }

#pragma warning disable CS0612 // Type or member is obsolete
                    if (!string.IsNullOrEmpty(settings.ThemeName))
                    {
                        settings.BackgroundColor = settings.ThemeName;
                        settings.ThemeName = null;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }
            return settings;
        }

        [JsonIgnore]
        public bool SettingsChangeRestartRequired { get; set; }

        [DataMember]
        public bool InstallerFolderUpgradeAsked { get; set; }

        [DataMember]
        public uint AutoLogInAccount { get; set; }

        [DataMember]
        public string ColorScheme { get; set; }

        [DataMember]
        [Obsolete]
        public string ThemeName { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }

        [DataMember]
        public string FullThemeName { get; set; }

        [DataMember]
        public string Language { get; set; }

        [JsonIgnore]
        public bool IsDarkBackground { get { return App.AppSettings.BackgroundColor.Equals("Dark"); } }

        public ApplicationSettings()
        {
            this.AutoLogInAccount = 0;
            this.ColorScheme = "Indigo";
            this.BackgroundColor = "Light";
            this.FullThemeName = "None";
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
