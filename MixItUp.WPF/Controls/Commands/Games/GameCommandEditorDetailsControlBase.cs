using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands.Games
{
    public class GameCommandEditorDetailsControlBase : CommandEditorDetailsControlBase
    {
        protected void OutcomeCommandButtons_CommandButtons(object sender, RoutedEventArgs e)
        {
            GameOutcomeViewModel outcome = FrameworkElementHelpers.GetDataContext<GameOutcomeViewModel>(sender);
            if (outcome.Command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(outcome.Command);
                window.CommandSaved += (object s, CommandModelBase command) => { outcome.Command = (CustomCommandModel)command; };
                window.ForceShow();
            }
        }

        protected void DeleteOutcomeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GameCommandEditorWindowViewModelBase viewModel = (GameCommandEditorWindowViewModelBase)this.DataContext;
            viewModel.DeleteOutcomeCommand.Execute(FrameworkElementHelpers.GetDataContext<GameOutcomeViewModel>(sender));
        }
    }
}
