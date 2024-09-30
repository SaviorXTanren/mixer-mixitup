using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Dialogs;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for CommandImporterDialogControl.xaml
    /// </summary>
    public partial class CommandImporterDialogControl : UserControl
    {
        private CommandModelBase command;

        public CommandImporterDialogControlViewModel ViewModel { get; private set; }

        public CommandImporterDialogControl(CommandModelBase command)
        {
            InitializeComponent();

            this.command = command;

            this.DataContext = this.ViewModel = new CommandImporterDialogControlViewModel();
        }

        private void CreateNewCommandTextBlock_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { this.ViewModel.IsNewCommandSelected = true; }

        private void AddToExistingCommandTextBlock_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { this.ViewModel.IsExistingCommandSelected = true; }

        private async void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel.IsExistingCommandSelected && this.ViewModel.SelectedExistingCommand == null)
            {
                return;
            }

            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(true, this);

            await Task.Delay(500);

            SettingsV3Upgrader.UpdateActionsV7(ChannelSession.Settings, this.command.Actions);

            if (this.ViewModel.IsNewCommandSelected)
            {
                if (this.ViewModel.SelectedNewCommandType == this.command.Type)
                {
                    this.command.ID = Guid.NewGuid();
                    CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(this.command);
                    window.CommandSaved += Window_CommandSaved;
                    window.ForceShow();
                }
                else
                {
                    CommandEditorWindow window = new CommandEditorWindow(this.ViewModel.SelectedNewCommandType, this.command.Name, this.command.Actions);
                    window.CommandSaved += Window_CommandSaved;
                    window.ForceShow();
                }
            }
            else if (this.ViewModel.IsExistingCommandSelected)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(this.ViewModel.SelectedExistingCommand, this.command.Actions);
                window.CommandSaved += Window_CommandSaved;
                window.ForceShow();
            }
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            GroupedCommandsMainControlViewModelBase.CommandAddedEdited(command);
        }
    }
}
