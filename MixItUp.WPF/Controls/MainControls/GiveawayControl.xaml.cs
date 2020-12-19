using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using MixItUp.WPF.Windows.Commands;
using MixItUp.WPF.Util;
using MixItUp.Base.Model.Commands;

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

        private void GiveawayCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private void GiveawayStartedReminderCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayStartedReminderCommand = command; };
            window.Show();
        }

        private void GiveawayUserJoinedCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayUserJoinedCommand = command; };
            window.Show();
        }

        private void GiveawayWinnerSelectedCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GiveawayMainControlViewModel)this.DataContext).GiveawayWinnerSelectedCommand = command; };
            window.Show();
        }
    }
}