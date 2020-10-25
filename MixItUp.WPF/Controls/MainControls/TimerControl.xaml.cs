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
            TimerCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<TimerCommandModel>();
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
                TimerCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<TimerCommandModel>();
                if (command != null)
                {
                    ChannelSession.TimerCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.FullRefresh();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Timer);
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
