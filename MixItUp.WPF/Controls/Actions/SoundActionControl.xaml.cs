using MixItUp.Base;
using MixItUp.Base.Actions;
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

        public SoundActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public SoundActionControl(ActionContainerControl containerControl, SoundAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.SoundFilePathTextBox.Text = this.action.FilePath;
                this.SoundVolumeSlider.Value = this.action.VolumeScale;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.SoundFilePathTextBox.Text))
            {
                return new SoundAction(this.SoundFilePathTextBox.Text, (int)this.SoundVolumeSlider.Value);
            }
            return null;
        }

        private void SoundFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SoundFilePathTextBox.Text = filePath;
            }
        }
    }
}
