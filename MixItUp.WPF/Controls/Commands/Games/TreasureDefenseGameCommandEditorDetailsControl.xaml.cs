using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for TreasureDefenseGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class TreasureDefenseGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public TreasureDefenseGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void UserJoinCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).UserJoinCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void NotEnoughPlayersCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).NotEnoughPlayersCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void KnightUserCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).KnightUserCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void ThiefUserCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).ThiefUserCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void KingUserCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).KingUserCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void KnightSelectedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).KnightSelectedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void ThiefSelectedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((TreasureDefenseGameCommandEditorWindowViewModel)this.DataContext).ThiefSelectedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
