using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GiveawayControl.xaml
    /// </summary>
    public partial class GiveawayControl : MainControlBase
    {
        private GiveawayMainControlViewModel viewModel;

        public GiveawayControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new GiveawayMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            return base.InitializeInternal();
        }

        private void GiveawayStartedReminderCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayStartedReminderCommand = command; };
            window.ForceShow();
        }

        private void GiveawayUserJoinedCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayUserJoinedCommand = command; };
            window.ForceShow();
        }

        private void GiveawayWinnerSelectedCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayWinnerSelectedCommand = command; };
            window.ForceShow();
        }
    }
}