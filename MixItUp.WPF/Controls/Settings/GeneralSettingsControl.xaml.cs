using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : SettingsControlBase
    {
        private const string FFMPEGDownloadLink = "https://ffmpeg.zeranoe.com/builds/win32/static/ffmpeg-4.0-win32-static.zip";

        public static readonly string OptOutTrackingTooltip =
            "This option allows you to opt-out of any tracking of your" + Environment.NewLine +
            "data via our back-end services. We track data around app" + Environment.NewLine +
            "usage & crashes to help improve your experience. If you" + Environment.NewLine +
            "select this option, we will stop all tracking of this data." + Environment.NewLine + Environment.NewLine +
            "Note that this does not disable any data included with uploading" + Environment.NewLine +
            "& reviewing Mix It Up store commands.";

        public static readonly string UpdatePreviewProgramTooltip =
            "The Update Preview Program allows you to get early access" + Environment.NewLine +
            "to new features & improvements before the general public." + Environment.NewLine +
            "This is a great way to give early feedback & provide valuable" + Environment.NewLine +
            "help. Preview Program updates can have bugs & issues associated" + Environment.NewLine +
            "with them, so users should only sign up for this if they are" + Environment.NewLine +
            "willing to work around any possible issues that may arise";

        public static readonly string AutoLogInTooltip =
            "The Auto Log-In setting allows Mix It Up to automatically" + Environment.NewLine +
            "log in the currently signed-in Mixer user account." + Environment.NewLine + Environment.NewLine +
            "NOTE: If there is a Mix It Up update or you are required" + Environment.NewLine +
            "to re-authenticate any of your services accounts, you will" + Environment.NewLine +
            "need to manually log in when this occurs.";

        private Dictionary<int, string> audioOutputDevices = new Dictionary<int, string>();

        public GeneralSettingsControl()
        {
            InitializeComponent();

            this.OptOutTrackingTextBlock.ToolTip = this.OptOutTrackingToggleButton.ToolTip = OptOutTrackingTooltip;

            this.UpdatePreviewProgramTextBlock.ToolTip = this.UpdatePreviewProgramToggleButton.ToolTip = UpdatePreviewProgramTooltip;

            this.AutoLogInAccountTextBlock.ToolTip = this.AutoLogInAccountToggleButton.ToolTip = AutoLogInTooltip;

            this.DefaultStreamingSoftwareComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingSoftwareTypeEnum>(
                new List<StreamingSoftwareTypeEnum>() { StreamingSoftwareTypeEnum.OBSStudio, StreamingSoftwareTypeEnum.XSplit, StreamingSoftwareTypeEnum.StreamlabsOBS });
        }

        protected override async Task InitializeInternal()
        {
            this.audioOutputDevices = await ChannelSession.Services.AudioService.GetOutputDevices();

            List<string> audioOutputDevicesNames = new List<string>();
            audioOutputDevicesNames.Add(SoundAction.DefaultAudioDevice);
            audioOutputDevicesNames.AddRange(this.audioOutputDevices.Values);
            this.DefaultAudioOutputComboBox.ItemsSource = audioOutputDevicesNames;

            this.OptOutTrackingToggleButton.IsChecked = ChannelSession.Settings.OptOutTracking;
            this.FeatureMeToggleButton.IsChecked = ChannelSession.Settings.FeatureMe;
            this.UpdatePreviewProgramToggleButton.IsChecked = App.AppSettings.PreviewProgram;
            this.AutoLogInAccountToggleButton.IsChecked = (App.AppSettings.AutoLogInAccount == ChannelSession.Channel.user.id);
            this.DefaultStreamingSoftwareComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.DefaultStreamingSoftware);
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultAudioOutput))
            {
                this.DefaultAudioOutputComboBox.SelectedItem = ChannelSession.Settings.DefaultAudioOutput;
            }
            else
            {
                this.DefaultAudioOutputComboBox.SelectedItem = SoundAction.DefaultAudioDevice;
            }
            this.SaveChatEventLogsToggleButton.IsChecked = ChannelSession.Settings.SaveChatEventLogs;

            this.CheckFFMPEGInstallation();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void OptOutTrackingToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.OptOutTracking = this.OptOutTrackingToggleButton.IsChecked.GetValueOrDefault();
            if (ChannelSession.Settings.OptOutTracking)
            {
                this.FeatureMeToggleButton.IsEnabled = false;
                this.FeatureMeToggleButton.IsChecked = false;
            }
            else
            {
                this.FeatureMeToggleButton.IsEnabled = true;
            }
        }

        private void FeatureMeToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.FeatureMe = this.FeatureMeToggleButton.IsChecked.GetValueOrDefault();
        }

        private void UpdatePreviewProgramToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            App.AppSettings.PreviewProgram = this.UpdatePreviewProgramToggleButton.IsChecked.GetValueOrDefault();
        }

        private void AutoLogInAccountToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.AppSettings.AutoLogInAccount = ChannelSession.Channel.user.id;
        }

        private void AutoLogInAccountToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.AppSettings.AutoLogInAccount = 0;
        }

        private void DefaultStreamingSoftwareComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DefaultStreamingSoftwareComboBox.SelectedIndex >= 0)
            {
                ChannelSession.Settings.DefaultStreamingSoftware = EnumHelper.GetEnumValueFromString<StreamingSoftwareTypeEnum>((string)this.DefaultStreamingSoftwareComboBox.SelectedItem);
            }
        }

        private void DefaultAudioOutputComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DefaultAudioOutputComboBox.SelectedIndex >= 0)
            {
                string audioDeviceName = (string)this.DefaultAudioOutputComboBox.SelectedItem;
                if (audioDeviceName.Equals(SoundAction.DefaultAudioDevice))
                {
                    ChannelSession.Settings.DefaultAudioOutput = null;
                }
                else
                {
                    ChannelSession.Settings.DefaultAudioOutput = audioDeviceName;
                }
            }
        }

        private async void DownloadAndInstallFFMPEGButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string zipFilePath = Path.Combine(ChannelSession.Services.FileService.GetTempFolder(), "ffmpeg.zip");

                await Task.Run(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(GeneralSettingsControl.FFMPEGDownloadLink), zipFilePath);
                    }
                });

                await ChannelSession.Services.FileService.UnzipFiles(zipFilePath, ChannelSession.Services.FileService.GetApplicationDirectory());
            });

            this.CheckFFMPEGInstallation();
        }

        private void CheckFFMPEGInstallation()
        {
            bool mmpegExists = ChannelSession.Services.FileService.FileExists(MixerClipsAction.GetFFMPEGExecutablePath());
            this.DownloadAndInstallFFMPEGButton.Visibility = (mmpegExists) ? Visibility.Collapsed : Visibility.Visible;
            this.FFMPEGInstalledTextBlock.Visibility = (mmpegExists) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveChatEventLogsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.SaveChatEventLogs = this.SaveChatEventLogsToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
