using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
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

        private void NewEventCommandButton_Click(object sender, RoutedEventArgs e)
        {
            EventCommandItemViewModel eventCommand = (EventCommandItemViewModel)((Button)sender).DataContext;
            CommandEditorWindow window = new CommandEditorWindow(eventCommand.EventType);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            EventCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<EventCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = new CommandEditorWindow(command);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                EventCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<EventCommandModel>();
                if (command != null)
                {
                    ChannelSession.EventCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.RefreshCommands();
                }
            });
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshCommands();
        }
    }
}
