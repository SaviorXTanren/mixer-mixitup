using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class IndividualNotificationSettingViewModel : UIViewModelBase
    {
        private const string SoundFilePathFormat = "Assets\\Sounds\\{0}.mp3";

        private static readonly string NoneSoundName = MixItUp.Base.Resources.None;
        private static readonly string CustomSoundName = MixItUp.Base.Resources.Custom;

        public string Name { get; set; }

        public List<string> Sounds { get; set; } = new List<string>() { NoneSoundName, "Ariel", "Carme", "Ceres", "Computer Chime", "Doorbell", "Elara", "Europa", "High Beeps", "Io",
            "Lapetus", "Level Up", "Low Beeps", "Rhea", "Robot SMS", "Salacia", "Tethys", "Titan", "Watch Alarm", CustomSoundName };

        public string Sound
        {
            get { return this.sound; }
            set
            {
                this.sound = value;
                this.NotifyPropertyChanged();

                if (this.Sound.Equals(NoneSoundName))
                {
                    this.valueSetter(null);
                }
                else if (this.Sound.Equals(CustomSoundName))
                {
                    string selectedSound = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().SoundFileFilter());
                    if (!string.IsNullOrEmpty(selectedSound))
                    {
                        this.valueSetter(selectedSound);
                    }
                    else
                    {
                        this.valueSetter(NoneSoundName);
                    }
                }
                else
                {
                    this.valueSetter(string.Format(SoundFilePathFormat, this.Sound));
                }
            }
        }
        private string sound;
        private Action<string> valueSetter;
        private Func<string> valueGetter;

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.volumeSetter(value);
                this.NotifyPropertyChanged();
            }
        }
        private int volume;
        private Action<int> volumeSetter;

        public ICommand PlayCommand { get; set; }

        public IndividualNotificationSettingViewModel(string name, Action<string> valueSetter, Func<string> valueGetter, int initialVolume, Action<int> volumeSetter)
        {
            this.Name = name;

            this.valueSetter = valueSetter;
            this.valueGetter = valueGetter;

            string soundPath = this.valueGetter();
            if (string.IsNullOrEmpty(soundPath))
            {
                this.sound = NoneSoundName;
            }
            else
            {
                foreach (string availableSound in this.Sounds)
                {
                    if (string.Format(SoundFilePathFormat, availableSound).Equals(soundPath))
                    {
                        this.sound = availableSound;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(this.sound))
                {
                    this.sound = CustomSoundName;
                }
            }

            this.volume = initialVolume;
            this.volumeSetter = volumeSetter;

            this.PlayCommand = this.CreateCommand(async () =>
            {
                string sound = this.valueGetter();
                if (!string.IsNullOrEmpty(sound))
                {
                    await ServiceManager.Get<IAudioService>().Play(sound, this.Volume, ChannelSession.Settings.NotificationsAudioOutput);
                }
            });
        }
    }

    public class NotificationsSettingsControlViewModel : UIViewModelBase
    {
        public GenericComboBoxSettingsOptionControlViewModel<string> NotificationsAudioOutput { get; set; }
        public GenericNumberSettingsOptionControlViewModel NotificationsCooldownAmount { get; set; }

        public ObservableCollection<IndividualNotificationSettingViewModel> NotificationSounds { get; set; } = new ObservableCollection<IndividualNotificationSettingViewModel>();

        public NotificationsSettingsControlViewModel()
        {
            string defaultAudioOption = ServiceManager.Get<IAudioService>().DefaultAudioDevice;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationsAudioOutput))
            {
                defaultAudioOption = ChannelSession.Settings.NotificationsAudioOutput;
            }

            this.NotificationsAudioOutput = new GenericComboBoxSettingsOptionControlViewModel<string>(MixItUp.Base.Resources.NotificationsAudioOutput,
                ServiceManager.Get<IAudioService>().GetSelectableAudioDevices(includeOverlay: true), defaultAudioOption, (value) =>
                {
                    if (value.Equals(ServiceManager.Get<IAudioService>().DefaultAudioDevice))
                    {
                        ChannelSession.Settings.NotificationsAudioOutput = null;
                    }
                    else
                    {
                        ChannelSession.Settings.NotificationsAudioOutput = value;
                    }
                });
            this.NotificationsAudioOutput.Width = 250;

            this.NotificationsCooldownAmount = new GenericNumberSettingsOptionControlViewModel(MixItUp.Base.Resources.NotificationCooldownAmount,
                ChannelSession.Settings.NotificationCooldownAmount, (value) => { ChannelSession.Settings.NotificationCooldownAmount = value; }, MixItUp.Base.Resources.Seconds);

            this.NotificationSounds.Add(new IndividualNotificationSettingViewModel(MixItUp.Base.Resources.AnyChatMessageSound,
                (value) => { ChannelSession.Settings.NotificationChatMessageSoundFilePath = value; }, () => { return ChannelSession.Settings.NotificationChatMessageSoundFilePath; },
                ChannelSession.Settings.NotificationChatMessageSoundVolume, (value) => { ChannelSession.Settings.NotificationChatMessageSoundVolume = value; }));

            this.NotificationSounds.Add(new IndividualNotificationSettingViewModel(MixItUp.Base.Resources.ChatTaggedSound,
                (value) => { ChannelSession.Settings.NotificationChatTaggedSoundFilePath = value; }, () => { return ChannelSession.Settings.NotificationChatTaggedSoundFilePath; },
                ChannelSession.Settings.NotificationChatTaggedSoundVolume, (value) => { ChannelSession.Settings.NotificationChatTaggedSoundVolume = value; }));

            this.NotificationSounds.Add(new IndividualNotificationSettingViewModel(MixItUp.Base.Resources.ChatWhisperSound,
                (value) => { ChannelSession.Settings.NotificationChatWhisperSoundFilePath = value; }, () => { return ChannelSession.Settings.NotificationChatWhisperSoundFilePath; },
                ChannelSession.Settings.NotificationChatWhisperSoundVolume, (value) => { ChannelSession.Settings.NotificationChatWhisperSoundVolume = value; }));

            this.NotificationSounds.Add(new IndividualNotificationSettingViewModel(MixItUp.Base.Resources.ServiceConnectSound,
                (value) => { ChannelSession.Settings.NotificationServiceConnectSoundFilePath = value; }, () => { return ChannelSession.Settings.NotificationServiceConnectSoundFilePath; },
                ChannelSession.Settings.NotificationServiceConnectSoundVolume, (value) => { ChannelSession.Settings.NotificationServiceConnectSoundVolume = value; }));

            this.NotificationSounds.Add(new IndividualNotificationSettingViewModel(MixItUp.Base.Resources.ServiceDisconnectSound,
                (value) => { ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath = value; }, () => { return ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath; },
                ChannelSession.Settings.NotificationServiceDisconnectSoundVolume, (value) => { ChannelSession.Settings.NotificationServiceDisconnectSoundVolume = value; }));
        }
    }
}