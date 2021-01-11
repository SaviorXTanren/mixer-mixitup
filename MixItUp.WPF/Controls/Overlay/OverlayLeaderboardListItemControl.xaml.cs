using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayLeaderboardListItemControl.xaml
    /// </summary>
    public partial class OverlayLeaderboardListItemControl : OverlayItemControl
    {
        public OverlayLeaderboardListItemControl()
        {
            InitializeComponent();
        }

        public OverlayLeaderboardListItemControl(OverlayLeaderboardListItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            await base.OnLoaded();
        }

        private void CreateNewLeaderCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand("New Leader")));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            ((OverlayLeaderboardListItemViewModel)this.ViewModel).NewLeaderCommand = (CustomCommand)e;
        }

        private void NewLeader_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private void NewLeaderCommand_DeleteClicked(object sender, RoutedEventArgs e)
        {
            ((OverlayLeaderboardListItemViewModel)this.ViewModel).NewLeaderCommand = null;
        }
    }
}
