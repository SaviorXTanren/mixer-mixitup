using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlaySongRequestsItemControl.xaml
    /// </summary>
    public partial class OverlayStreamBossItemControl : OverlayItemControl
    {
        public OverlayStreamBossItemControl()
        {
            InitializeComponent();
        }

        public OverlayStreamBossItemControl(OverlayStreamBossItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            await base.OnLoaded();
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(OverlayStreamBossItemModel.NewStreamBossCommandName)));
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
                ((OverlayStreamBossItemViewModel)this.ViewModel).NewBossCommand = null;
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            ((OverlayStreamBossItemViewModel)this.ViewModel).NewBossCommand = (CustomCommand)e;
        }
    }
}
