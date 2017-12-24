using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
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
    public partial class TimerControl : MainControlBase
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

            GlobalEvents.OnCommandUpdated += GlobalEvents_OnCommandUpdated;
            GlobalEvents.OnCommandDeleted += GlobalEvents_OnCommandDeleted;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.timerCommands.Clear();
            foreach (TimerCommand command in ChannelSession.Settings.TimerCommands)
            {
                this.timerCommands.Add(command);
            }
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

        private async void GlobalEvents_OnCommandUpdated(object sender, CommandBase e)
        {
            if (e is TimerCommand)
            {
                ChannelSession.Settings.TimerCommands.Remove((TimerCommand)e);

                await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.TimerCommandsListView.SelectedIndex = -1;

                this.RefreshList();
            }
        }

        private void GlobalEvents_OnCommandDeleted(object sender, CommandBase e)
        {
            if (e is TimerCommand)
            {
                ChannelSession.Settings.TimerCommands.Remove((TimerCommand)e);

                this.GlobalEvents_OnCommandUpdated(sender, e);
            }
        }
    }
}
