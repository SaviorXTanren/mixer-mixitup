using MixItUp.Base;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for ChatSettingsControl.xaml
    /// </summary>
    public partial class ChatSettingsControl : SettingsControlBase
    {
        public const int ChatDefaultFontSize = 13;

        private Dictionary<string, int> fontSizes = new Dictionary<string, int>() { { "Normal", ChatDefaultFontSize }, { "Large", 16 }, { "XLarge", 20 }, { "XXLarge", 24 }, };

        public ChatSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.TrackWhispererNumberToggleButton.IsEnabled = ChannelSession.IsStreamer;

            this.FontSizeComboBox.ItemsSource = this.fontSizes.Keys;

            this.FontSizeComboBox.SelectedItem = this.fontSizes.FirstOrDefault(f => f.Value == ChannelSession.Settings.ChatFontSize).Key;

            this.LatestChatAtTopToggleButton.IsChecked = ChannelSession.Settings.LatestChatAtTop;
            this.HideViewerAndChatterNumbersToggleButton.IsChecked = ChannelSession.Settings.HideViewerAndChatterNumbers;
            this.HideChatUserListToggleButton.IsChecked = ChannelSession.Settings.HideChatUserList;
            this.HideDeletedMessagesToggleButton.IsChecked = ChannelSession.Settings.HideDeletedMessages;
            this.TrackWhispererNumberToggleButton.IsChecked = ChannelSession.Settings.TrackWhispererNumber;
            this.ShowMessageTimestampsToggleButton.IsChecked = ChannelSession.Settings.ShowChatMessageTimestamps;
            this.ShowBetterTTVEmotesToggleButton.IsChecked = ChannelSession.Settings.ShowBetterTTVEmotes;
            this.ShowFrankerFaceZEmotesToggleButton.IsChecked = ChannelSession.Settings.ShowFrankerFaceZEmotes;

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FontSizeComboBox.SelectedIndex >= 0)
            {
                string name = (string)this.FontSizeComboBox.SelectedItem;
                ChannelSession.Settings.ChatFontSize = this.fontSizes[name];
                GlobalEvents.ChatFontSizeChanged();
            }
        }

        private void LatestChatAtTopToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.LatestChatAtTop = this.LatestChatAtTopToggleButton.IsChecked.GetValueOrDefault();
        }

        private void HideViewerAndChatterNumbersToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.HideViewerAndChatterNumbers = this.HideViewerAndChatterNumbersToggleButton.IsChecked.GetValueOrDefault();
        }

        private void HideChatUserListToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.HideChatUserList = this.HideChatUserListToggleButton.IsChecked.GetValueOrDefault();
        }

        private void HideDeletedMessagesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.HideDeletedMessages = this.HideDeletedMessagesToggleButton.IsChecked.GetValueOrDefault();
        }

        private void TrackWhispererNumberToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.TrackWhispererNumber = this.TrackWhispererNumberToggleButton.IsChecked.GetValueOrDefault();
        }

        private void ShowBetterTTVEmotesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ShowBetterTTVEmotes = this.ShowBetterTTVEmotesToggleButton.IsChecked.GetValueOrDefault();
        }

        private void ShowFrankerFaceZEmotesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ShowFrankerFaceZEmotes = this.ShowFrankerFaceZEmotesToggleButton.IsChecked.GetValueOrDefault();
        }

        private void ShowMessageTimestampsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ShowChatMessageTimestamps = this.ShowMessageTimestampsToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
