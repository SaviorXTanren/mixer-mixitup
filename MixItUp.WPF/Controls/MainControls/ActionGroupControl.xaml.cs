using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ActionGroupControl.xaml
    /// </summary>
    public partial class ActionGroupControl : GroupedCommandsMainControlBase
    {
        private ActionGroupMainControlViewModel viewModel;

        public ActionGroupControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new ActionGroupMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.SetViewModel(this.viewModel);
            await this.viewModel.OnOpen();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await base.OnVisibilityChanged();
            this.NameFilterTextBox.Text = string.Empty;
        }

        private void NameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.NameFilter = this.NameFilterTextBox.Text;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            ActionGroupCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<ActionGroupCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.CommandSaved += Window_CommandSaved;
                window.ForceShow();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ActionGroupCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<ActionGroupCommandModel>();
                if (command != null)
                {
                    ServiceManager.Get<CommandService>().ActionGroupCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    this.viewModel.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.ActionGroup);
            window.CommandSaved += Window_CommandSaved;
            window.Show();
        }
    }
}
