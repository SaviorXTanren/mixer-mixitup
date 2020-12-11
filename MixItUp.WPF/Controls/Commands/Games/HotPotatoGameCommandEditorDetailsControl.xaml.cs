using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Commands.Games
{
    /// <summary>
    /// Interaction logic for HotPotatoGameCommandEditorDetailsControl.xaml
    /// </summary>
    public partial class HotPotatoGameCommandEditorDetailsControl : GameCommandEditorDetailsControlBase
    {
        public HotPotatoGameCommandEditorDetailsControl()
        {
            InitializeComponent();
        }

        private void StartedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HotPotatoGameCommandEditorWindowViewModel)this.DataContext).StartedCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void TossPotatoCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HotPotatoGameCommandEditorWindowViewModel)this.DataContext).TossPotatoCommand = (CustomCommandModel)command; };
            window.Show();
        }

        private void PotatoExplodeCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((HotPotatoGameCommandEditorWindowViewModel)this.DataContext).PotatoExplodeCommand = (CustomCommandModel)command; };
            window.Show();
        }
    }
}
