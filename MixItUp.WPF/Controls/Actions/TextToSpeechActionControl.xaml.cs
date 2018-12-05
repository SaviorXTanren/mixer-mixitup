using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for TextToSpeechActionControl.xaml
    /// </summary>
    public partial class TextToSpeechActionControl : ActionControlBase
    {
        private TextToSpeechAction action;

        private ObservableCollection<string> voices = new ObservableCollection<string>();

        public TextToSpeechActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public TextToSpeechActionControl(ActionContainerControl containerControl, TextToSpeechAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.TextToSpeechVoiceComboBox.ItemsSource = this.voices;

            this.OverlayNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.OverlayServers == null) ? Visibility.Visible : Visibility.Collapsed;

            foreach (string voice in TextToSpeechAction.AvailableVoices)
            {
                this.voices.Add(voice);
            }

            if (this.action != null)
            {
                this.TextToSpeechMessageTextBox.Text = this.action.SpeechText;
                this.TextToSpeechVoiceComboBox.SelectedItem = this.action.Voice;
                this.TextToSpeechVolumeTextBox.Text = (this.action.Volume * 100.0).ToString();
                this.TextToSpeechPitchTextBox.Text = (this.action.Pitch * 100.0).ToString();
                this.TextToSpeechRateTextBox.Text = (this.action.Rate * 100.0).ToString();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.TextToSpeechMessageTextBox.Text) && this.TextToSpeechVoiceComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.TextToSpeechVolumeTextBox.Text)
                && !string.IsNullOrEmpty(this.TextToSpeechPitchTextBox.Text) && !string.IsNullOrEmpty(this.TextToSpeechRateTextBox.Text))
            {
                if (double.TryParse(this.TextToSpeechVolumeTextBox.Text.Replace("%", ""), out double volume) && double.TryParse(this.TextToSpeechPitchTextBox.Text.Replace("%", ""), out double pitch) &&
                    double.TryParse(this.TextToSpeechRateTextBox.Text.Replace("%", ""), out double rate))
                {
                    if (volume >= 0.0 && volume <= 100.0 && pitch >= 0.0 && pitch <= 200.0 && rate >= 0.0 && rate <= 150.0)
                    {
                        return new TextToSpeechAction(this.TextToSpeechMessageTextBox.Text, (string)this.TextToSpeechVoiceComboBox.SelectedItem, (volume / 100.0), (pitch / 100.0), (rate / 100.0));
                    }
                }
            }
            return null;
        }
    }
}
