using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            return Task.CompletedTask;
        }

        private void NewEventCommandButton_Click(object sender, RoutedEventArgs e)
        {
            EventCommandItemViewModel item = FrameworkElementHelpers.GetDataContext<EventCommandItemViewModel>(sender);
            CommandEditorWindow window = new CommandEditorWindow(item.EventType);
            window.Closed += (object s, System.EventArgs ee) =>
            {
                this.viewModel.EventTypeItems[item.EventType].RefreshCommand();
            };
            window.ForceShow();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandListingButtonsControl commandListingButtonsControl = ((CommandListingButtonsControl)sender);
            EventCommandModel command = commandListingButtonsControl.GetCommandFromCommandButtons<EventCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.Closed += (object s, System.EventArgs ee) =>
                {
                    this.viewModel.EventTypeItems[command.EventType].RefreshCommand();
                    commandListingButtonsControl.RefreshUI();
                };
                window.ForceShow();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandListingButtonsControl commandListingButtonsControl = ((CommandListingButtonsControl)sender);
                EventCommandModel command = commandListingButtonsControl.GetCommandFromCommandButtons<EventCommandModel>();
                if (command != null)
                {
                    ServiceManager.Get<CommandService>().EventCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();

                    this.viewModel.EventTypeItems[command.EventType].RefreshCommand();
                    commandListingButtonsControl.RefreshUI();
                }
            });
        }

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
