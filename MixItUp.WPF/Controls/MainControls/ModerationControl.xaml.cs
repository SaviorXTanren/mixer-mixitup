using MixItUp.Base;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ModerationControl.xaml
    /// </summary>
    public partial class ModerationControl : MainControlBase
    {
        public ModerationControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.BannedWordsTextBox.Text = string.Join(Environment.NewLine, ChannelSession.Settings.BannedWords);

            this.MaxCapsAllowedSlider.Value = ChannelSession.Settings.ModerationCapsBlockCount;
            this.MaxPunctuationAllowedSlider.Value = ChannelSession.Settings.ModerationPunctuationBlockCount;
            this.MaxEmoteAllowedSlider.Value = ChannelSession.Settings.ModerationEmoteBlockCount;
            this.BlockLinksToggleButton.IsChecked = ChannelSession.Settings.ModerationBlockLinks;
            this.IncludeModeratorsToggleButton.IsChecked = ChannelSession.Settings.ModerationIncludeModerators;
            this.Timeout1MinAfterSlider.Value = ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount;
            this.Timeout5MinAfterSlider.Value = ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount;

            return base.InitializeInternal();
        }

        private async void BannedWordsTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            string bannedWords = this.BannedWordsTextBox.Text;
            if (string.IsNullOrEmpty(this.BannedWordsTextBox.Text))
            {
                bannedWords = "";
            }

            ChannelSession.Settings.BannedWords.Clear();
            foreach (string split in bannedWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ChannelSession.Settings.BannedWords.Add(split.ToLower());
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.SaveSettings();
            });
        }

        private void MaxCapsAllowedSlider_ValueChanged(object sender, int e)
        {
            ChannelSession.Settings.ModerationCapsBlockCount = (int)this.MaxCapsAllowedSlider.Value;
        }

        private void MaxPunctuationAllowedSlider_ValueChanged(object sender, int e)
        {
            ChannelSession.Settings.ModerationPunctuationBlockCount = (int)this.MaxPunctuationAllowedSlider.Value;
        }

        private void MaxEmoteAllowedSlider_ValueChanged(object sender, int e)
        {
            ChannelSession.Settings.ModerationEmoteBlockCount = (int)this.MaxEmoteAllowedSlider.Value;
        }

        private async void BlockLinksToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.ModerationBlockLinks = BlockLinksToggleButton.IsChecked.GetValueOrDefault();
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.SaveSettings();
            });
        }

        private async void IncludeModeratorsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.ModerationIncludeModerators = IncludeModeratorsToggleButton.IsChecked.GetValueOrDefault();
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.SaveSettings();
            });
        }

        private void Timeout1MinAfterSlider_ValueChanged(object sender, int e)
        {
            ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount = (int)this.Timeout1MinAfterSlider.Value;
        }

        private void Timeout5MinAfterSlider_ValueChanged(object sender, int e)
        {
            ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount = (int)this.Timeout5MinAfterSlider.Value;
        }

        private async void Slider_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.SaveSettings();
            });
        }
    }
}
