using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for DuelGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class DuelGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public DuelGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((DuelGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void NotAcceptedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((DuelGameCommandEditorWindowViewModel)this.DataContext).NotAcceptedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void FailedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((DuelGameCommandEditorWindowViewModel)this.DataContext).FailedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
