using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Settings
{
    public class GeneralSettingsControlViewModel : UIViewModelBase
    {
        public GenericTextSettingsOptionControlViewModel ProfileName { get; set; }
        public GenericToggleSettingsOptionControlViewModel AutoLogIn { get; set; }

        public GenericComboBoxSettingsOptionControlViewModel<LanguageOptions> Language { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<StreamingPlatformTypeEnum> DefaultStreamingPlatform { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<StreamingSoftwareTypeEnum> DefaultStreamingSoftware { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<string> DefaultAudioOutput { get; set; }

        public GenericToggleSettingsOptionControlViewModel DontSaveLastWindowPosition { get; set; }
        public GenericToggleSettingsOptionControlViewModel OptOutOfDataTracking { get; set; }

        public GeneralSettingsControlViewModel()
        {
            this.ProfileName = new GenericTextSettingsOptionControlViewModel(MixItUp.Base.Resources.ProfileName,
                ChannelSession.Settings.Name,
                (value) => { ChannelSession.Settings.Name = value; });

            this.AutoLogIn = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AutoLogInCurrentAccount,
                (ChannelSession.AppSettings.AutoLogInID == ChannelSession.Settings.ID),
                (value) => { ChannelSession.AppSettings.AutoLogInID = (value) ? ChannelSession.Settings.ID : Guid.Empty; },
                MixItUp.Base.Resources.AutoLogInCurrentAccountTooltip);


            var languageOptions = EnumHelper.GetEnumList<LanguageOptions>().ToList();
            if (!ChannelSession.IsDebug())
            {
                languageOptions.Remove(LanguageOptions.Pseudo);
            }
            this.Language = new GenericComboBoxSettingsOptionControlViewModel<LanguageOptions>(MixItUp.Base.Resources.Language,
                languageOptions.OrderBy(l => l.ToString()), ChannelSession.AppSettings.LanguageOption, (value) =>
                {
                    ChannelSession.AppSettings.SettingsChangeRestartRequired = true;
                    ChannelSession.AppSettings.LanguageOption = value;
                });

            this.DefaultStreamingPlatform = new GenericComboBoxSettingsOptionControlViewModel<StreamingPlatformTypeEnum>(MixItUp.Base.Resources.DefaultStreamingPlatform,
                StreamingPlatforms.SupportedPlatforms, ChannelSession.Settings.DefaultStreamingPlatform, (value) => { ChannelSession.Settings.DefaultStreamingPlatform = value; });

            this.DefaultStreamingSoftware = new GenericComboBoxSettingsOptionControlViewModel<StreamingSoftwareTypeEnum>(MixItUp.Base.Resources.DefaultStreamingSoftware,
                new List<StreamingSoftwareTypeEnum>() { StreamingSoftwareTypeEnum.OBSStudio, StreamingSoftwareTypeEnum.XSplit, StreamingSoftwareTypeEnum.StreamlabsDesktop },
                ChannelSession.Settings.DefaultStreamingSoftware, (value) => { ChannelSession.Settings.DefaultStreamingSoftware = value; });

            string defaultAudioOption = ServiceManager.Get<IAudioService>().DefaultAudioDevice;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultAudioOutput))
            {
                defaultAudioOption = ChannelSession.Settings.DefaultAudioOutput;
            }

            this.DefaultAudioOutput = new GenericComboBoxSettingsOptionControlViewModel<string>(MixItUp.Base.Resources.DefaultAudioOutput,
                ServiceManager.Get<IAudioService>().GetSelectableAudioDevices(includeOverlay: true), defaultAudioOption, (value) =>
                {
                    if (value.Equals(ServiceManager.Get<IAudioService>().DefaultAudioDevice))
                    {
                        ChannelSession.Settings.DefaultAudioOutput = null;
                    }
                    else
                    {
                        ChannelSession.Settings.DefaultAudioOutput = value;
                    }
                });
            this.DefaultAudioOutput.Width = 250;


            this.DontSaveLastWindowPosition = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DontSaveLastWindowPosition,
                ChannelSession.AppSettings.DontSaveLastWindowPosition, (value) => { ChannelSession.AppSettings.DontSaveLastWindowPosition = value; });
            this.OptOutOfDataTracking = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.OptOutofDataTracking,
                ChannelSession.Settings.OptOutTracking, (value) => { ChannelSession.Settings.OptOutTracking = value; }, MixItUp.Base.Resources.OptOutofDataTrackingTooltip);
        }
    }
}
