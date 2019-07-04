using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayProgressBarControl.xaml
    /// </summary>
    public partial class OverlayProgressBarControl : OverlayItemControl
    {
        private OverlayProgressBarItemViewModel viewModel;

        public OverlayProgressBarControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayProgressBarItemViewModel();
        }

        public OverlayProgressBarControl(OverlayItemModelBase item)
        {
            InitializeComponent();

            this.viewModel = new OverlayProgressBarItemViewModel((OverlayProgressBarItemModel)item);
        }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
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
                this.viewModel.OnGoalReachedCommand = null;
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.OnGoalReachedCommand = (CustomCommand)e;
        }
    }
}
