using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for BetGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class BetGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public BetGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BetGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void UserJoinCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BetGameCommandEditorWindowViewModel)this.DataContext).UserJoinCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void NotEnoughPlayersCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BetGameCommandEditorWindowViewModel)this.DataContext).NotEnoughPlayersCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void BetsClosedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BetGameCommandEditorWindowViewModel)this.DataContext).BetsClosedCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void GameCompleteCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BetGameCommandEditorWindowViewModel)this.DataContext).GameCompleteCommand = (CustomCommandModel)command; };
            window.Show();
        }
    }
}
