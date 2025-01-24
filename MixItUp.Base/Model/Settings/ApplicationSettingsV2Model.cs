using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class ApplicationSettingsV2Model
    {
        private const string ApplicationSettingsFileName = "ApplicationSettings.json";

        public static async Task<ApplicationSettingsV2Model> Load()
        {
            ApplicationSettingsV2Model settings = null;
            if (ServiceManager.Get<IFileService>().FileExists(ApplicationSettingsFileName))
            {
                try
                {
                    settings = await FileSerializerHelper.DeserializeFromFile<ApplicationSettingsV2Model>(ApplicationSettingsFileName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            if (settings == null)
            {
                settings = new ApplicationSettingsV2Model();
            }

            if (settings.ForceResetPreviewProgram)
            {
                settings.ForceResetPreviewProgram = false;
                settings.PreviewProgram = false;
            }

            return settings;
        }

        [JsonIgnore]
        public bool SettingsChangeRestartRequired { get; set; }

        [DataMember]
        public bool DiagnosticLogging { get; set; } = false;

        [DataMember]
        public bool PreviewProgram { get; set; } = false;
        [DataMember]
        public bool ForceResetPreviewProgram { get; set; } = true;

        [DataMember]
        public bool TestBuild { get; set; } = false;

        [DataMember]
        public Guid AutoLogInID { get; set; } = Guid.Empty;

        [DataMember]
        public string ColorScheme { get; set; } = "Indigo";

        [DataMember]
        public string BackgroundColor { get; set; } = "Light";

        [DataMember]
        public string FullThemeName { get; set; } = string.Empty;

        [DataMember]
        public bool DontSaveLastWindowPosition { get; set; }

        [DataMember]
        public double Top { get; set; }

        [DataMember]
        public double Left { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public bool IsMaximized { get; set; }

        [DataMember]
        public double DashboardTop { get; set; }

        [DataMember]
        public double DashboardLeft { get; set; }

        [DataMember]
        public double DashboardWidth { get; set; }

        [DataMember]
        public double DashboardHeight { get; set; }

        [DataMember]
        public bool IsDashboardMaximized { get; set; }

        [DataMember]
        public bool IsDashboardPinned { get; set; }

        [DataMember]
        public LanguageOptions LanguageOption { get; set; }

        [DataMember]
        public string SettingsRestoreFilePath { get; set; }
        [DataMember]
        public Guid SettingsToReplaceDuringRestore { get; set; }

        [DataMember]
        public Guid SettingsToDelete { get; set; }

        [JsonIgnore]
        public bool IsDarkBackground { get { return this.BackgroundColor.Equals("Dark"); } }

        public ApplicationSettingsV2Model() { }

        public async Task Save()
        {
            try
            {
                await FileSerializerHelper.SerializeToFile(ApplicationSettingsFileName, this);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}