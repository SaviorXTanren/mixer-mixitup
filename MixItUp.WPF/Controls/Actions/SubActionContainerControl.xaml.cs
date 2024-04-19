using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.WPF.Controls.Dialogs;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SubActionContainerControl.xaml
    /// </summary>
    public partial class SubActionContainerControl : UserControl
    {
        public SubActionContainerControl()
        {
            InitializeComponent();
        }

        private async void CopyFromExitingCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandSelectorDialogControl dialogControl = new CommandSelectorDialogControl();
            if (bool.Equals(await DialogHelper.ShowCustom(dialogControl), true) && dialogControl.ViewModel.SelectedCommand != null)
            {
                await ((GroupActionEditorControlViewModel)this.DataContext).ImportActionsFromCommand(dialogControl.ViewModel.SelectedCommand);
            }
        }
    }
}
