using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        private EventsMainControlViewModel viewModel;

        public EventsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new EventsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            return Task.FromResult(0);
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            EventCommandItemViewModel eventCommand = (EventCommandItemViewModel)button.DataContext;
            CommandWindow window = new CommandWindow(new EventCommandDetailsControl(eventCommand.EventType));
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            EventCommand command = commandButtonsControl.GetCommandFromCommandButtons<EventCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new EventCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                EventCommand command = commandButtonsControl.GetCommandFromCommandButtons<EventCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.RefreshEventCommands();
                }
            });
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshEventCommands();
        }
    }
}
