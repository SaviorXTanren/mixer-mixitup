using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for LockBoxGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class LockBoxGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public LockBoxGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void SuccessCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((LockBoxGameCommandEditorWindowViewModel)this.DataContext).SuccessfulCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void FailureCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((LockBoxGameCommandEditorWindowViewModel)this.DataContext).FailureCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((LockBoxGameCommandEditorWindowViewModel)this.DataContext).StatusCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void InspectionCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((LockBoxGameCommandEditorWindowViewModel)this.DataContext).InspectionCommand = (CustomCommandModel)command; };
            window.Show();
        }
    }
}
