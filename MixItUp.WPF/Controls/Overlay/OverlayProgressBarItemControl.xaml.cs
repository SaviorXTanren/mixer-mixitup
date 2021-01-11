using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
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

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(OverlayProgressBarItemModel.GoalReachedCommandName)));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                ((OverlayProgressBarItemViewModel)this.ViewModel).OnGoalReachedCommand = null;
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            ((OverlayProgressBarItemViewModel)this.ViewModel).OnGoalReachedCommand = (CustomCommand)e;
        }
    }
}
