using MixItUp.Base.Actions;
using StreamingClient.Base.Util;
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
            if (this.action != null)
            {
                this.ActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ActionType);
                
                if (this.action.ActionType == StreamingPlatformActionType.Host)
                {
                    this.HostChannelNameTextBox.Text = this.action.HostChannelName;
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
                else if (actionType == StreamingPlatformActionType.RunAd)
                {
                    return StreamingPlatformAction.CreateRunAdAction();
                }
            }
            return null;
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ActionTypeComboBox.SelectedIndex >= 0)
            {
                this.HostGrid.Visibility = Visibility.Collapsed;
                StreamingPlatformActionType actionType = EnumHelper.GetEnumValueFromString<StreamingPlatformActionType>((string)this.ActionTypeComboBox.SelectedItem);
                if (actionType == StreamingPlatformActionType.Host)
                {
                    this.HostGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
