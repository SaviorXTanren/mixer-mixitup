using MixItUp.Base.Actions;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamingPlatformActionControl.xaml
    /// </summary>
    public partial class StreamingPlatformActionControl : ActionControlBase
    {
        private StreamingPlatformAction action;

        public StreamingPlatformActionControl() : base() { InitializeComponent(); }

        public StreamingPlatformActionControl(StreamingPlatformAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.ActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingPlatformActionType>().OrderBy(s => s);

            this.AdLengthComboBox.ItemsSource = new List<int>() { 30, 60, 90, 120, 150, 180 };
            this.AdLengthComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                this.ActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ActionType);
                
                if (this.action.ActionType == StreamingPlatformActionType.Host || this.action.ActionType == StreamingPlatformActionType.Raid)
                {
                    this.HostChannelNameTextBox.Text = this.action.HostChannelName;
                }
                else if (this.action.ActionType == StreamingPlatformActionType.RunAd)
                {
                    this.AdLengthComboBox.SelectedItem = this.action.AdLength;
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.ActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamingPlatformActionType actionType = EnumHelper.GetEnumValueFromString<StreamingPlatformActionType>((string)this.ActionTypeComboBox.SelectedItem);
                if (actionType == StreamingPlatformActionType.Host)
                {
                    if (!string.IsNullOrEmpty(this.HostChannelNameTextBox.Text))
                    {
                        return StreamingPlatformAction.CreateHostAction(this.HostChannelNameTextBox.Text);
                    }
                }
                else if (actionType == StreamingPlatformActionType.Raid)
                {
                    if (!string.IsNullOrEmpty(this.HostChannelNameTextBox.Text))
                    {
                        return StreamingPlatformAction.CreateRaidAction(this.HostChannelNameTextBox.Text);
                    }
                }
                else if (actionType == StreamingPlatformActionType.RunAd)
                {
                    if (this.AdLengthComboBox.SelectedIndex >= 0)
                    {
                        return StreamingPlatformAction.CreateRunAdAction((int)this.AdLengthComboBox.SelectedItem);
                    }
                }
            }
            return null;
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ActionTypeComboBox.SelectedIndex >= 0)
            {
                this.HostChannelNameTextBox.Visibility = Visibility.Collapsed;
                this.AdLengthComboBox.Visibility = Visibility.Collapsed;

                StreamingPlatformActionType actionType = EnumHelper.GetEnumValueFromString<StreamingPlatformActionType>((string)this.ActionTypeComboBox.SelectedItem);
                if (actionType == StreamingPlatformActionType.Host || actionType == StreamingPlatformActionType.Raid)
                {
                    this.HostChannelNameTextBox.Visibility = Visibility.Visible;
                }
                else if (actionType == StreamingPlatformActionType.RunAd)
                {
                    this.AdLengthComboBox.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
