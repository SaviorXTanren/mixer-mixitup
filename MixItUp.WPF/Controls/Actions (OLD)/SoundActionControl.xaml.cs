using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SoundActionControl.xaml
    /// </summary>
    public partial class SoundActionControl : ActionControlBase
    {
        private SoundAction action;

        private Dictionary<int, string> audioOutputDevices = new Dictionary<int, string>();

        public SoundActionControl() : base() { InitializeComponent(); }

        public SoundActionControl(SoundAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            List<string> audioOutputDevicesNames = new List<string>();
            audioOutputDevicesNames.Add(SoundAction.DefaultAudioDevice);
            audioOutputDevicesNames.Add(SoundAction.MixItUpOverlay);
            audioOutputDevicesNames.AddRange(ChannelSession.Services.AudioService.GetOutputDevices());
            this.AudioOutputComboBox.ItemsSource = audioOutputDevicesNames;

            this.AudioOutputComboBox.SelectedIndex = 0;
            this.SoundVolumeSlider.Value = 100;
            if (this.action != null)
            {
                this.SoundFilePathTextBox.Text = this.action.FilePath;
                if (!string.IsNullOrEmpty(this.action.OutputDevice))
                {
                    this.AudioOutputComboBox.SelectedItem = this.action.OutputDevice;
                }
                this.SoundVolumeSlider.Value = this.action.VolumeScale;
            }

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.SoundFilePathTextBox.Text))
            {
                string audioOutputDevice = null;
                if (this.AudioOutputComboBox.SelectedIndex > 0)
                {
                    audioOutputDevice = (string)this.AudioOutputComboBox.SelectedItem;
                }

                return new SoundAction(this.SoundFilePathTextBox.Text, (int)this.SoundVolumeSlider.Value, audioOutputDevice);
            }
            return null;
        }

        private void SoundFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.MusicFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SoundFilePathTextBox.Text = filePath;
            }
        }
    }
}
