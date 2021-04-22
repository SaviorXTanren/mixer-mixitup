using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for StealGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class StealGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public StealGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void FailedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((StealGameCommandEditorWindowViewModel)this.DataContext).FailedCommand = (CustomCommandModel)command; };
            window.ForceShow();
        }
    }
}
