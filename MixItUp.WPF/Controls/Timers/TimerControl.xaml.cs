using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Timers
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

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TimerCommand command = (TimerCommand)button.DataContext;

            await this.Window.RunAsyncOperation(async () =>
            {
                await command.Perform();
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TimerCommand command = (TimerCommand)button.DataContext;

            CommandWindow window = new CommandWindow(new TimerCommandDetailsControl(command));
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TimerCommand command = (TimerCommand)button.DataContext;
            ChannelSession.Settings.TimerCommands.Remove(command);

            await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

            this.TimerCommandsListView.SelectedIndex = -1;

            this.RefreshList();
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = true;
        }

        private void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = false;
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

        private void TimerMinimumMessagesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(this.TimerMinimumMessagesTextBox.Text, out value) && value > 0)
            {
                ChannelSession.Settings.TimerCommandsMinimumMessages = value;
            }
            else
            {
                MessageBoxHelper.ShowDialog("Minimum Messages must be greater than 0");
                this.TimerMinimumMessagesTextBox.Text = ChannelSession.Settings.TimerCommandsMinimumMessages.ToString();
            }
        }

        private void TimerIntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(this.TimerIntervalTextBox.Text, out value) && value >= 0)
            {
                ChannelSession.Settings.TimerCommandsInterval = value;
            }
            else
            {
                MessageBoxHelper.ShowDialog("Interval must be 0 or greater");
                this.TimerIntervalTextBox.Text = ChannelSession.Settings.TimerCommandsInterval.ToString();
            }
        }
    }
}
