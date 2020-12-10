using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for SlotMachineGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class SlotMachineGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public SlotMachineGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void FailureCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += Window_CommandSaved;
            window.Show();
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            ((SlotMachineGameCommandEditorWindowViewModel)this.DataContext).FailureCommand = (CustomCommandModel)command;
        }
    }
}
