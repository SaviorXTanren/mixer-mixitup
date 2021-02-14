using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for BigGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class BidGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public BidGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BidGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void NewTopBidderCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BidGameCommandEditorWindowViewModel)this.DataContext).NewTopBidderCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void NotEnoughPlayersCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BidGameCommandEditorWindowViewModel)this.DataContext).NotEnoughPlayersCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void GameCompleteCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((BidGameCommandEditorWindowViewModel)this.DataContext).GameCompleteCommand = (CustomCommandModel)command; };
            window.Show();
        }
    }
}
