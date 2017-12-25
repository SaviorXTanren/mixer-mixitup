using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for TimerControl.xaml
    /// </summary>
    public partial class TimerControl : MainCommandControlBase
    {
        private ObservableCollection<TimerCommand> timerCommands = new ObservableCollection<TimerCommand>();

        public TimerControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.TimerCommandsListView.ItemsSource = this.timerCommands;
            this.TimerIntervalTextBox.Text = ChannelSession.Settings.TimerCommandsInterval.ToString();
            this.TimerMinimumMessagesTextBox.Text = ChannelSession.Settings.TimerCommandsMinimumMessages.ToString();

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.TimerCommandsListView.SelectedIndex = -1;

            this.timerCommands.Clear();
            foreach (TimerCommand command in ChannelSession.Settings.TimerCommands)
            {
                this.timerCommands.Add(command);
            }
        }

        private async void CommandButtons_PlayClicked(object sender, RoutedEventArgs e)
        {
            await this.HandleCommandPlay(sender);
        }

        private void CommandButtons_StopClicked(object sender, RoutedEventArgs e)
        {
            this.HandleCommandStop(sender);
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            TimerCommand command = this.GetCommandFromCommandButtons<TimerCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new TimerCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                TimerCommand command = this.GetCommandFromCommandButtons<TimerCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.TimerCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.HandleCommandEnableDisable(sender);
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new TimerCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }

        private async void TimerMinimumMessagesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(this.TimerMinimumMessagesTextBox.Text, out value) && value > 0)
            {
                ChannelSession.Settings.TimerCommandsMinimumMessages = value;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Minimum Messages must be greater than 0");
                this.TimerMinimumMessagesTextBox.Text = ChannelSession.Settings.TimerCommandsMinimumMessages.ToString();
            }
        }

        private async void TimerIntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(this.TimerIntervalTextBox.Text, out value) && value >= 0)
            {
                ChannelSession.Settings.TimerCommandsInterval = value;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Interval must be 0 or greater");
                this.TimerIntervalTextBox.Text = ChannelSession.Settings.TimerCommandsInterval.ToString();
            }
        }
    }
}
