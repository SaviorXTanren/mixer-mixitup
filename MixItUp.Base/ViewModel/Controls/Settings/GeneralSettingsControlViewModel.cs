using MixItUp.Base.Actions;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.ViewModel.Controls.Settings.Generic;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class GeneralSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel OptOutOfDataTracking { get; set; }
        public GenericToggleSettingsOptionControlViewModel AutoLogIn { get; set; }
        public GenericToggleSettingsOptionControlViewModel PreviewProgram { get; set; }

        public GenericCombBoxSettingsOptionControlViewModel<LanguageOptions> Language { get; set; }
        public GenericCombBoxSettingsOptionControlViewModel<StreamingSoftwareTypeEnum> DefaultStreamingSoftware { get; set; }
        public GenericCombBoxSettingsOptionControlViewModel<string> DefaultAudioOutput { get; set; }

        public GeneralSettingsControlViewModel()
        {
            this.OptOutOfDataTracking = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.OptOutofDataTracking,
                ChannelSession.Settings.OptOutTracking, (value) => { ChannelSession.Settings.OptOutTracking = value; }, MixItUp.Base.Resources.OptOutofDataTrackingTooltip);

            this.AutoLogIn = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AutoLogInCurrentAccount,
                (ChannelSession.AppSettings.AutoLogInID == ChannelSession.Settings.ID),
                (value) => { ChannelSession.AppSettings.AutoLogInID = (value) ? ChannelSession.Settings.ID : Guid.Empty; },
                MixItUp.Base.Resources.AutoLogInCurrentAccountTooltip);

            this.PreviewProgram = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UpdatePreviewProgram,
                ChannelSession.AppSettings.PreviewProgram, (value) => { ChannelSession.AppSettings.PreviewProgram = value; }, MixItUp.Base.Resources.UpdatePreviewProgramTooltip);

            var languageOptions = EnumHelper.GetEnumList<LanguageOptions>().ToList();
            if (!ChannelSession.IsDebug())
            {
                languageOptions.Remove(LanguageOptions.Pseudo);
            }
            this.Language = new GenericCombBoxSettingsOptionControlViewModel<LanguageOptions>(MixItUp.Base.Resources.Language,
                languageOptions.OrderBy(l => l.ToString()), ChannelSession.AppSettings.LanguageOption, (value) =>
                {
                    ChannelSession.AppSettings.SettingsChangeRestartRequired = true;
                    ChannelSession.AppSettings.LanguageOption = value;
                });

            this.DefaultStreamingSoftware = new GenericCombBoxSettingsOptionControlViewModel<StreamingSoftwareTypeEnum>(MixItUp.Base.Resources.DefaultStreamingSoftware,
                new List<StreamingSoftwareTypeEnum>() { StreamingSoftwareTypeEnum.OBSStudio, StreamingSoftwareTypeEnum.XSplit, StreamingSoftwareTypeEnum.StreamlabsOBS },
                ChannelSession.Settings.DefaultStreamingSoftware, (value) => { ChannelSession.Settings.DefaultStreamingSoftware = value; });

            string defaultAudioOption = SoundAction.DefaultAudioDevice;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultAudioOutput))
            {
                defaultAudioOption = ChannelSession.Settings.DefaultAudioOutput;
            }

            List<string> audioOptions = new List<string>();
            audioOptions.Add(SoundAction.DefaultAudioDevice);
            audioOptions.AddRange(ChannelSession.Services.AudioService.GetOutputDevices());

            this.DefaultAudioOutput = new GenericCombBoxSettingsOptionControlViewModel<string>(MixItUp.Base.Resources.DefaultAudioOutput,
                audioOptions, defaultAudioOption, (value) =>
                {
                    if (value.Equals(SoundAction.DefaultAudioDevice))
                    {
                        ChannelSession.Settings.DefaultAudioOutput = null;
                    }
                    else
                    {
                        ChannelSession.Settings.DefaultAudioOutput = value;
                    }
                });
            this.DefaultAudioOutput.Width = 250;
        }
    }
}
