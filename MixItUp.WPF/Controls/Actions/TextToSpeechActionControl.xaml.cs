using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for TextToSpeechActionControl.xaml
    /// </summary>
    public partial class TextToSpeechActionControl : ActionControlBase
    {
        private TextToSpeechAction action;

        private ObservableCollection<TextToSpeechVoice> voices = new ObservableCollection<TextToSpeechVoice>();

        public TextToSpeechActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public TextToSpeechActionControl(ActionContainerControl containerControl, TextToSpeechAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.TextToSpeechVoiceComboBox.ItemsSource = this.voices;
            this.TextToSpeechRateComboBox.ItemsSource = EnumHelper.GetEnumNames<SpeechRate>();
            this.TextToSpeechVolumeComboBox.ItemsSource = EnumHelper.GetEnumNames<SpeechVolume>();

            foreach (TextToSpeechVoice voice in ChannelSession.Services.TextToSpeechService.GetInstalledVoices())
            {
                this.voices.Add(voice);
            }

            if (this.action != null)
            {
                this.TextToSpeechMessageTextBox.Text = this.action.SpeechText;
                this.TextToSpeechVoiceComboBox.SelectedItem = this.action.SpeechVoice;
                this.TextToSpeechRateComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.SpeechRate);
                this.TextToSpeechVolumeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.SpeechVolume);
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.TextToSpeechMessageTextBox.Text) && this.TextToSpeechVoiceComboBox.SelectedIndex >= 0
                && this.TextToSpeechVolumeComboBox.SelectedIndex >= 0 && this.TextToSpeechRateComboBox.SelectedIndex >= 0)
            {
                TextToSpeechVoice voice = (TextToSpeechVoice)this.TextToSpeechVoiceComboBox.SelectedItem;
                SpeechVolume volume = EnumHelper.GetEnumValueFromString<SpeechVolume>(this.TextToSpeechVolumeComboBox.Text);
                SpeechRate rate = EnumHelper.GetEnumValueFromString<SpeechRate>(this.TextToSpeechRateComboBox.Text);
                return new TextToSpeechAction(this.TextToSpeechMessageTextBox.Text, voice, rate, volume);
            }
            return null;
        }
    }
}
