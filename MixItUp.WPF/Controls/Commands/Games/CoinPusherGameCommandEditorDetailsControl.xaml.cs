using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for CoinPusherGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class CoinPusherGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public CoinPusherGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void SuccessCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((CoinPusherGameCommandEditorWindowViewModel)this.DataContext).SuccessCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void FailureCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((CoinPusherGameCommandEditorWindowViewModel)this.DataContext).FailureCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((CoinPusherGameCommandEditorWindowViewModel)this.DataContext).StatusCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
