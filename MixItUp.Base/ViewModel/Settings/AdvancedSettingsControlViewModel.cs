using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using System.IO;

namespace MixItUp.Base.ViewModel.Settings
{
    public class AdvancedSettingsControlViewModel : UIViewModelBase
    {
        public GenericButtonSettingsOptionControlViewModel BackupSettings { get; set; }
        public GenericButtonSettingsOptionControlViewModel RestoreSettings { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<SettingsBackupRateEnum> AutomaticBackupRate { get; set; }
        public GenericButtonSettingsOptionControlViewModel AutomaticBackupLocation { get; set; }

        public string AutomaticBackupLocationFolderPath { get { return !string.IsNullOrEmpty(ChannelSession.Settings.SettingsBackupLocation) ? ChannelSession.Settings.SettingsBackupLocation : MixItUp.Base.Resources.MixItUpInstallFolder; } }

        public GenericToggleSettingsOptionControlViewModel PreviewProgram { get; set; }

        public GenericButtonSettingsOptionControlViewModel InstallationFolder { get; set; }
        public GenericToggleSettingsOptionControlViewModel DiagnosticLogging { get; set; }
        public GenericButtonSettingsOptionControlViewModel RunNewUserWizard { get; set; }
        public GenericButtonSettingsOptionControlViewModel DeleteSettings { get; set; }

        public AdvancedSettingsControlViewModel()
        {
            this.BackupSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.BackupYourCurrentSettings, MixItUp.Base.Resources.BackupSettings, this.CreateCommand(async () =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowSaveFileDialog(ChannelSession.Settings.Name + "." + SettingsV3Model.SettingsBackupFileExtension, MixItUp.Base.Resources.MixItUpBackupFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ServiceManager.Get<SettingsService>().SavePackagedBackup(ChannelSession.Settings, filePath);
                }
            }));

            this.RestoreSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.RestoreASettingsBackup, MixItUp.Base.Resources.RestoreSettings, this.CreateCommand(async () =>
            {
                await SettingsV3Model.RestoreSettingsBackup();
            }));

            this.AutomaticBackupRate = new GenericComboBoxSettingsOptionControlViewModel<SettingsBackupRateEnum>(MixItUp.Base.Resources.AutomatedSettingsBackupRate, EnumHelper.GetEnumList<SettingsBackupRateEnum>(),
                ChannelSession.Settings.SettingsBackupRate, (value) => { ChannelSession.Settings.SettingsBackupRate = value; });

            this.AutomaticBackupLocation = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.AutomatedSettingsBackupLocation, MixItUp.Base.Resources.SetLocation, this.CreateCommand(() =>
            {
                string folderPath = ServiceManager.Get<IFileService>().ShowOpenFolderDialog();
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    ChannelSession.Settings.SettingsBackupLocation = folderPath;
                    this.NotifyPropertyChanged("AutomaticBackupLocationFolderPath");
                }
            }));

            this.PreviewProgram = new GenericToggleSettingsOptionControlViewModel(
                MixItUp.Base.Resources.UpdatePreviewProgram,
                ChannelSession.AppSettings.PreviewProgram,
                async (value) =>
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.UpdatePreviewProgramTooltip);
                    ChannelSession.AppSettings.PreviewProgram = value;
                },
                MixItUp.Base.Resources.UpdatePreviewProgramTooltip);

            this.InstallationFolder = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.AccessTheFolderWhereMixItUpIsInstalled, MixItUp.Base.Resources.InstallationFolder, this.CreateCommand(() =>
            {
                ServiceManager.Get<IProcessService>().LaunchFolder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            }));

            this.DiagnosticLogging = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DiagnosticLogging, ChannelSession.AppSettings.DiagnosticLogging,
                (value) => { ChannelSession.AppSettings.DiagnosticLogging = value; }, MixItUp.Base.Resources.DiagnosticLoggingToolip);

            this.RunNewUserWizard = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ReRunNewUserWizard, MixItUp.Base.Resources.NewUserWizard, this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.RunNewUserWizardWarning))
                {
                    ChannelSession.Settings.ReRunWizard = true;
                    ChannelSession.RestartRequested();
                }
            }));

            this.DeleteSettings = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.DeleteCurrentSettingsData, MixItUp.Base.Resources.DeleteSettings, this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(Resources.DeleteSettingsWarning))
                {
                    ChannelSession.AppSettings.SettingsToDelete = ChannelSession.Settings.ID;
                    ChannelSession.RestartRequested();
                }
            }));
        }
    }
}
