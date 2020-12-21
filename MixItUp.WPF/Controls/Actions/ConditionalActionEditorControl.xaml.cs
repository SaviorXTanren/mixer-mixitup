using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.WPF.Controls.Dialogs;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ConditionalActionEditorControl.xaml
    /// </summary>
    public partial class ConditionalActionEditorControl : ActionEditorControlBase
    {
        public ConditionalActionEditorControl()
        {
            InitializeComponent();
        }

        private async void CopyFromExitingCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandSelectorDialogControl dialogControl = new CommandSelectorDialogControl();
            if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && dialogControl.ViewModel.SelectedCommand != null)
            {
                await ((ConditionalActionEditorControlViewModel)this.DataContext).ImportActionsFromCommand(dialogControl.ViewModel.SelectedCommand);
            }
        }
    }
}
