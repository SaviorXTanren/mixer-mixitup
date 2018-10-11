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

        private Dictionary<string, int> fontSizes = new Dictionary<string, int>() { { "Normal", ChatDefaultFontSize }, { "Large", 16 }, { "X-Large", 20 }, { "XX-Large", 24 }, };

        public ChatSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.UserJoinLeaveColorSchemeComboBox.AddDefaultOption();
            this.UserJoinLeaveColorSchemeComboBox.SelectionChanged += UserJoinLeaveColorSchemeComboBox_SelectionChanged;
            this.EventAlertsColorSchemeComboBox.AddDefaultOption();
            this.EventAlertsColorSchemeComboBox.SelectionChanged += EventAlertsColorSchemeComboBox_SelectionChanged;
            this.InteractiveAlertsColorSchemeComboBox.AddDefaultOption();
            this.InteractiveAlertsColorSchemeComboBox.SelectionChanged += InteractiveAlertsColorSchemeComboBox_SelectionChanged;

            this.FontSizeComboBox.ItemsSource = this.fontSizes.Keys;

            this.FontSizeComboBox.SelectedItem = this.fontSizes.FirstOrDefault(f => f.Value == ChannelSession.Settings.ChatFontSize).Key;

            this.ShowUserJoinLeaveToggleButton.IsChecked = ChannelSession.Settings.ChatShowUserJoinLeave;
            this.UserJoinLeaveColorSchemeComboBox.SelectedItem = this.UserJoinLeaveColorSchemeComboBox.AvailableColorSchemes.FirstOrDefault(c => c.Name.Equals(ChannelSession.Settings.ChatUserJoinLeaveColorScheme));
            this.ShowEventAlertsToggleButton.IsChecked = ChannelSession.Settings.ChatShowEventAlerts;
            this.EventAlertsColorSchemeComboBox.SelectedItem = this.EventAlertsColorSchemeComboBox.AvailableColorSchemes.FirstOrDefault(c => c.Name.Equals(ChannelSession.Settings.ChatEventAlertsColorScheme));
            this.ShowInteractiveAlertsToggleButton.IsChecked = ChannelSession.Settings.ChatShowInteractiveAlerts;
            this.InteractiveAlertsColorSchemeComboBox.SelectedItem = this.InteractiveAlertsColorSchemeComboBox.AvailableColorSchemes.FirstOrDefault(c => c.Name.Equals(ChannelSession.Settings.ChatInteractiveAlertsColorScheme));

            this.WhisperAllAlertsToggleButton.IsChecked = ChannelSession.Settings.WhisperAllAlerts;
            this.LatestChatAtTopToggleButton.IsChecked = ChannelSession.Settings.LatestChatAtTop;
            this.HideViewerAndChatterNumbersToggleButton.IsChecked = ChannelSession.Settings.HideViewerAndChatterNumbers;
            this.HideDeletedMessagesToggleButton.IsChecked = ChannelSession.Settings.HideDeletedMessages;
            this.TrackWhispererNumberToggleButton.IsChecked = ChannelSession.Settings.TrackWhispererNumber;
            this.AllowCommandWhisperingToggleButton.IsChecked = ChannelSession.Settings.AllowCommandWhispering;
            this.IgnoreBotAccountCommandsToggleButton.IsChecked = ChannelSession.Settings.IgnoreBotAccountCommands;
            this.CommandsOnlyInYourStreamToggleButton.IsChecked = ChannelSession.Settings.CommandsOnlyInYourStream;
            this.DeleteChatCommandsWhenRunToggleButton.IsChecked = ChannelSession.Settings.DeleteChatCommandsWhenRun;

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

        private void ShowUserJoinLeaveToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ChatShowUserJoinLeave = this.ShowUserJoinLeaveToggleButton.IsChecked.GetValueOrDefault();
            this.UserJoinLeaveColorSchemeComboBox.IsEnabled = ChannelSession.Settings.ChatShowUserJoinLeave;
        }

        private void UserJoinLeaveColorSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.UserJoinLeaveColorSchemeComboBox.SelectedIndex >= 0)
            {
                ColorSchemeOption colorScheme = (ColorSchemeOption)this.UserJoinLeaveColorSchemeComboBox.SelectedItem;
                ChannelSession.Settings.ChatUserJoinLeaveColorScheme = colorScheme.Name;
            }
        }

        private void ShowEventAlertsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ChatShowEventAlerts = this.ShowEventAlertsToggleButton.IsChecked.GetValueOrDefault();
            this.EventAlertsColorSchemeComboBox.IsEnabled = ChannelSession.Settings.ChatShowEventAlerts;
        }

        private void EventAlertsColorSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.EventAlertsColorSchemeComboBox.SelectedIndex >= 0)
            {
                ColorSchemeOption colorScheme = (ColorSchemeOption)this.EventAlertsColorSchemeComboBox.SelectedItem;
                ChannelSession.Settings.ChatEventAlertsColorScheme = colorScheme.Name;
            }
        }

        private void ShowInteractiveAlertsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ChatShowInteractiveAlerts = this.ShowInteractiveAlertsToggleButton.IsChecked.GetValueOrDefault();
            this.InteractiveAlertsColorSchemeComboBox.IsEnabled = ChannelSession.Settings.ChatShowInteractiveAlerts;
        }

        private void InteractiveAlertsColorSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveAlertsColorSchemeComboBox.SelectedIndex >= 0)
            {
                ColorSchemeOption colorScheme = (ColorSchemeOption)this.InteractiveAlertsColorSchemeComboBox.SelectedItem;
                ChannelSession.Settings.ChatInteractiveAlertsColorScheme = colorScheme.Name;
            }
        }

        private void WhisperAllAlertsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.WhisperAllAlerts = this.WhisperAllAlertsToggleButton.IsChecked.GetValueOrDefault();
        }

        private void LatestChatAtTopToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.LatestChatAtTop = this.LatestChatAtTopToggleButton.IsChecked.GetValueOrDefault();
        }

        private void HideViewerAndChatterNumbersToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.HideViewerAndChatterNumbers = this.HideViewerAndChatterNumbersToggleButton.IsChecked.GetValueOrDefault();
        }

        private void TrackWhispererNumberToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.TrackWhispererNumber = this.TrackWhispererNumberToggleButton.IsChecked.GetValueOrDefault();
        }

        private void AllowCommandWhisperingToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.AllowCommandWhispering = this.AllowCommandWhisperingToggleButton.IsChecked.GetValueOrDefault();
        }

        private void IgnoreBotAccountCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.IgnoreBotAccountCommands = this.IgnoreBotAccountCommandsToggleButton.IsChecked.GetValueOrDefault();
        }

        private void CommandsOnlyInYourStreamToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.CommandsOnlyInYourStream = this.CommandsOnlyInYourStreamToggleButton.IsChecked.GetValueOrDefault();
        }

        private void DeleteChatCommandsWhenRunToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.DeleteChatCommandsWhenRun = this.DeleteChatCommandsWhenRunToggleButton.IsChecked.GetValueOrDefault();
        }

        private void HideDeletedMessagesToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.HideDeletedMessages = this.HideDeletedMessagesToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
