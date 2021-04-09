using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for HangmanGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class HangmanGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public HangmanGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void SuccessfulGuessCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HangmanGameCommandEditorWindowViewModel)this.DataContext).SuccessfulGuessCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void FailedGuessCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HangmanGameCommandEditorWindowViewModel)this.DataContext).FailedGuessCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void GameWonCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HangmanGameCommandEditorWindowViewModel)this.DataContext).GameWonCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void GameLostCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HangmanGameCommandEditorWindowViewModel)this.DataContext).GameLostCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HangmanGameCommandEditorWindowViewModel)this.DataContext).StatusCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
