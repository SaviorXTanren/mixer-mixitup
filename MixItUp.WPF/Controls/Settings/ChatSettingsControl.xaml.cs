using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.MainControls;
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
    public partial class ChatSettingsControl : MainControlBase
    {
        private Dictionary<string, int> fontSizes = new Dictionary<string, int>() { { "Normal", 13 }, { "Large", 16 }, { "X-Large", 20 }, { "XX-Large", 24 }, };

        public ChatSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.FontSizeComboBox.ItemsSource = this.fontSizes.Keys;
            this.FontSizeComboBox.SelectedItem = this.fontSizes.FirstOrDefault(f => f.Value == ChannelSession.Settings.ChatFontSize).Key;
            this.ShowUserJoinLeaveToggleButton.IsChecked = ChannelSession.Settings.ChatShowUserJoinLeave;
            this.ShowEventAlertsToggleButton.IsChecked = ChannelSession.Settings.ChatShowEventAlerts;
            this.ShowInteractiveAlertsToggleButton.IsChecked = ChannelSession.Settings.ChatShowInteractiveAlerts;

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
        }

        private void ShowEventAlertsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ChatShowEventAlerts = this.ShowEventAlertsToggleButton.IsChecked.GetValueOrDefault();
        }

        private void ShowInteractiveAlertsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.ChatShowInteractiveAlerts = this.ShowInteractiveAlertsToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
