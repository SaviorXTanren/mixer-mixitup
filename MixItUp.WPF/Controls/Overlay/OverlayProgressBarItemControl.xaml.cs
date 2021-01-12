using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayProgressBarItemControl.xaml
    /// </summary>
    public partial class OverlayProgressBarItemControl : OverlayItemControl
    {
        public OverlayProgressBarItemControl()
        {
            InitializeComponent();
        }

        public OverlayProgressBarItemControl(OverlayProgressBarItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.ProgressBarGoalReached);
            window.CommandSaved += Window_CommandSaved;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = new CommandEditorWindow(command);
                window.CommandSaved += Window_CommandSaved;
                window.Show();
            }
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                ((OverlayLeaderboardListItemViewModel)this.ViewModel).NewLeaderCommand = null;
            }
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            ((OverlayProgressBarItemViewModel)this.ViewModel).OnGoalReachedCommand = (CustomCommandModel)command;
        }
    }
}
