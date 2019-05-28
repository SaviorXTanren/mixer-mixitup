using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for TimerControl.xaml
    /// </summary>
    public partial class TimerControl : GroupedCommandsMainControlBase
    {
        private TimerMainControlViewModel viewModel;

        public TimerControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new TimerMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.SetViewModel(this.viewModel);
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        private void NameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.NameFilter = this.NameFilterTextBox.Text;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            TimerCommand command = commandButtonsControl.GetCommandFromCommandButtons<TimerCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new TimerCommandDetailsControl(command));
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                TimerCommand command = commandButtonsControl.GetCommandFromCommandButtons<TimerCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.TimerCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.RemoveCommand(command);
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new TimerCommandDetailsControl());
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }
    }
}
