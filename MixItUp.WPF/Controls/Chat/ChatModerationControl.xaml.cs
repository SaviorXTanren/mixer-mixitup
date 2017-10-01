using MixItUp.Base;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatModerationControl.xaml
    /// </summary>
    public partial class ChatModerationControl : MainControlBase
    {
        public ChatModerationControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.BannedWordsTextBox.Text = string.Join(Environment.NewLine, ChannelSession.Settings.BannedWords);

            this.MaxCapsAllowedSlider.Value = ChannelSession.Settings.CapsBlockCount;
            this.MaxEmoteSymbolAllowedSlider.Value = ChannelSession.Settings.SymbolEmoteBlockCount;
            this.BlockLinksCheckBox.IsChecked = ChannelSession.Settings.BlockLinks;

            this.Timeout1MinAfterSlider.Value = ChannelSession.Settings.Timeout1MinuteOffenseCount;
            this.Timeout5MinAfterSlider.Value = ChannelSession.Settings.Timeout5MinuteOffenseCount;

            return base.InitializeInternal();
        }

        private void BannedWordsTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            string bannedWords = this.BannedWordsTextBox.Text;
            if (string.IsNullOrEmpty(this.BannedWordsTextBox.Text))
            {
                bannedWords = "";
            }

            ChannelSession.Settings.BannedWords.Clear();
            foreach (string split in bannedWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ChannelSession.Settings.BannedWords.Add(split);
            }
        }

        private void MaxCapsAllowedSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ChannelSession.Settings.CapsBlockCount = (int)this.MaxCapsAllowedSlider.Value;
        }

        private void MaxEmoteSymbolAllowedSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ChannelSession.Settings.SymbolEmoteBlockCount = (int)this.MaxEmoteSymbolAllowedSlider.Value;
        }

        private void BlockLinksCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e) { ChannelSession.Settings.BlockLinks = true; }

        private void BlockLinksCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e) { ChannelSession.Settings.BlockLinks = false; }

        private void Timeout1MinAfterSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ChannelSession.Settings.Timeout1MinuteOffenseCount = (int)this.Timeout1MinAfterSlider.Value;
        }

        private void Timeout5MinAfterSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ChannelSession.Settings.Timeout5MinuteOffenseCount = (int)this.Timeout5MinAfterSlider.Value;
        }
    }
}
