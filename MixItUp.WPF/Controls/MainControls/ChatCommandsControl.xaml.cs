using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : GroupedCommandsMainControlBase
    {
        private ChatCommandsMainControlViewModel viewModel;

        public ChatCommandsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new ChatCommandsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.SetViewModel(this.viewModel);
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();

            await base.OnVisibilityChanged();
        }

        protected override void Window_CommandSaveSuccessfully(object sender, CommandBase command)
        {
            ChannelSession.Services.Chat.RebuildCommandTriggers();

            base.Window_CommandSaveSuccessfully(sender, command);
        }

        private void NameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.NameFilter = this.NameFilterTextBox.Text;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command));
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.ChatCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.RemoveCommand(command);

                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            });
        }

        private void CommandButtonsControl_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl());
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
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
