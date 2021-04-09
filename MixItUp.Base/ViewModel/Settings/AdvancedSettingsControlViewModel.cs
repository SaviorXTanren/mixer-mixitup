using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.IO;

namespace MixItUp.Base.ViewModel.Settings
{
    public class AdvancedSettingsControlViewModel : UIViewModelBase
    {
        public GenericButtonSettingsOptionControlViewModel BackupSettings { get; set; }
        public GenericButtonSettingsOptionControlViewModel RestoreSettings { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<SettingsBackupRateEnum> AutomaticBackupRate { get; set; }
        public GenericButtonSettingsOptionControlViewModel AutomaticBackupLocation { get; set; }

        public GenericButtonSettingsOptionControlViewModel InstallationFolder { get; set; }
        public GenericToggleSettingsOptionControlViewModel DiagnosticLogging { get; set; }
        public GenericButtonSettingsOptionControlViewModel RunNewUserWizard { get; set; }
        public GenericButtonSettingsOptionControlViewModel DeleteSettings { get; set; }

        public AdvancedSettingsControlViewModel()
        {
            this.BackupSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.BackupYourCurrentSettings, MixItUp.Base.Resources.BackupSettings, this.CreateCommand(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(ChannelSession.Settings.Name + "." + SettingsV3Model.SettingsBackupFileExtension);
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ChannelSession.Services.Settings.SavePackagedBackup(ChannelSession.Settings, filePath);
                }
            }));

            this.RestoreSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.RestoreASettingsBackup, MixItUp.Base.Resources.RestoreSettings, this.CreateCommand(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(string.Format("Mix It Up Settings V2 Backup (*.{0})|*.{0}|All files (*.*)|*.*", SettingsV3Model.SettingsBackupFileExtension));
                if (!string.IsNullOrEmpty(filePath))
                {
                    Result<SettingsV3Model> result = await ChannelSession.Services.Settings.RestorePackagedBackup(filePath);
                    if (result.Success)
                    {
                        ChannelSession.AppSettings.BackupSettingsFilePath = filePath;
                        ChannelSession.AppSettings.BackupSettingsToReplace = ChannelSession.Settings.ID;
                        GlobalEvents.RestartRequested();
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
            }));

            this.AutomaticBackupRate = new GenericComboBoxSettingsOptionControlViewModel<SettingsBackupRateEnum>(MixItUp.Base.Resources.AutomatedSettingsBackupRate, EnumHelper.GetEnumList<SettingsBackupRateEnum>(),
                ChannelSession.Settings.SettingsBackupRate, (value) => { ChannelSession.Settings.SettingsBackupRate = value; });

            this.AutomaticBackupLocation = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.AutomatedSettingsBackupLocation, MixItUp.Base.Resources.SetLocation, this.CreateCommand(() =>
            {
                string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    ChannelSession.Settings.SettingsBackupLocation = folderPath;
                }
            }));

            this.InstallationFolder = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.AccessTheFolderWhereMixItUpIsInstalled, MixItUp.Base.Resources.InstallationFolder, this.CreateCommand(() =>
            {
                ProcessHelper.LaunchFolder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            }));

            this.DiagnosticLogging = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DiagnosticLogging, ChannelSession.AppSettings.DiagnosticLogging,
                (value) => { ChannelSession.AppSettings.DiagnosticLogging = value; }, MixItUp.Base.Resources.DiagnosticLoggingToolip);

            this.RunNewUserWizard = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ReRunNewUserWizard, MixItUp.Base.Resources.NewUserWizard, this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.RunNewUserWizardWarning))
                {
                    ChannelSession.Settings.ReRunWizard = true;
                    GlobalEvents.RestartRequested();
                }
            }));

            this.DeleteSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.DeleteCurrentSettingsData, MixItUp.Base.Resources.DeleteSettings, this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.DeleteSettingsWarning))
                {
                    ChannelSession.AppSettings.SettingsToDelete = ChannelSession.Settings.ID;
                    GlobalEvents.RestartRequested();
                }
            }));
        }
    }
}
