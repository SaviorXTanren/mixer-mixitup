using MixItUp.Base;
using MixItUp.Base.Actions;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for MixerClipsActionControl.xaml
    /// </summary>
    public partial class MixerClipsActionControl : ActionControlBase
    {
        private MixerClipsAction action;

        public MixerClipsActionControl() : base() { InitializeComponent(); }

        public MixerClipsActionControl(MixerClipsAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.ClipLengthTextBox.Text = "30";
            this.ShowClipInfoInChatToggleButton.IsChecked = true;

            this.OnlyAvailableForPartnersWarningTextBlock.Visibility = (ChannelSession.MixerChannel.partnered) ? Visibility.Collapsed : Visibility.Visible;

            bool mmpegExists = ChannelSession.Services.FileService.FileExists(MixerClipsAction.GetFFMPEGExecutablePath());
            if (!mmpegExists)
            {
                this.DownloadClipGrid.IsEnabled = false;
                this.DownloadDirectoryTextBox.Visibility = Visibility.Collapsed;
                this.DownloadDirectoryBrowseButton.Visibility = Visibility.Collapsed;
                this.FFMPEGNotInstalledTextBlock.Visibility = Visibility.Visible;
            }

            if (this.action != null)
            {
                this.ClipNameTextBox.Text = this.action.ClipName;
                this.ClipLengthTextBox.Text = this.action.ClipLength.ToString();
                this.ShowClipInfoInChatToggleButton.IsChecked = this.action.ShowClipInfoInChat;
                this.DownloadClipToggleButton.IsChecked = this.action.DownloadClip;
                this.DownloadDirectoryTextBox.Text = this.action.DownloadDirectory;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.ClipNameTextBox.Text) && !string.IsNullOrEmpty(this.ClipLengthTextBox.Text))
            {
                if (int.TryParse(this.ClipLengthTextBox.Text, out int length) && MixerClipsAction.MinimumLength <= length && length <= MixerClipsAction.MaximumLength)
                {
                    if (this.DownloadClipToggleButton.IsChecked.GetValueOrDefault())
                    {
                        if (!string.IsNullOrEmpty(this.DownloadDirectoryTextBox.Text) && Directory.Exists(this.DownloadDirectoryTextBox.Text))
                        {
                            return new MixerClipsAction(this.ClipNameTextBox.Text, length, showClipInfoInChat: this.ShowClipInfoInChatToggleButton.IsChecked.GetValueOrDefault(),
                                downloadClip: true, downloadDirectory: this.DownloadDirectoryTextBox.Text);
                        }
                    }
                    else
                    {
                        return new MixerClipsAction(this.ClipNameTextBox.Text, length, showClipInfoInChat: this.ShowClipInfoInChatToggleButton.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void DownloadClipToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.DownloadDirectoryTextBox.IsEnabled = this.DownloadDirectoryBrowseButton.IsEnabled = this.DownloadClipToggleButton.IsChecked.GetValueOrDefault();
        }

        private void DownloadDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
            if (!string.IsNullOrEmpty(directoryPath))
            {
                this.DownloadDirectoryTextBox.Text = directoryPath;
            }
        }
    }
}
