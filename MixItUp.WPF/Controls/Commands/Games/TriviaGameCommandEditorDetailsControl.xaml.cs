using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for TriviaGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class TriviaGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public TriviaGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TriviaGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void UserJoinCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TriviaGameCommandEditorWindowViewModel)this.DataContext).UserJoinCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void CorrectAnswerCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TriviaGameCommandEditorWindowViewModel)this.DataContext).CorrectAnswerCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void UserSuccessCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TriviaGameCommandEditorWindowViewModel)this.DataContext).UserSuccessCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void UserFailureCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TriviaGameCommandEditorWindowViewModel)this.DataContext).UserFailureCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void DeleteQuestionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TriviaGameCommandEditorWindowViewModel viewModel = (TriviaGameCommandEditorWindowViewModel)this.DataContext;
            viewModel.DeleteQuestionCommand.Execute(FrameworkElementHelpers.GetDataContext<TriviaGameQuestionViewModel>(sender));
        }
    }
}
