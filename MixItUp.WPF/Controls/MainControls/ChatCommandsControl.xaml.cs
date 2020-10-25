using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
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

        private void NameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.NameFilter = this.NameFilterTextBox.Text;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            ChatCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<ChatCommandModel>();
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
                ChatCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<ChatCommandModel>();
                if (command != null)
                {
                    ChannelSession.ChatCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.FullRefresh();
                }
            });
        }

        private void CommandButtonsControl_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Chat);
            window.Closed += Window_Closed;
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
