using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for VolcanoGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class VolcanoGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public VolcanoGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void Stage1DepositCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage1DepositCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void Stage1StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage1StatusCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void Stage2DepositCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage2DepositCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void Stage2StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage2StatusCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void Stage3DepositCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage3DepositCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void Stage3StatusCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).Stage3StatusCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void PayoutCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).PayoutCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }

        private void CollectCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((VolcanoGameCommandEditorWindowViewModel)this.DataContext).CollectCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
