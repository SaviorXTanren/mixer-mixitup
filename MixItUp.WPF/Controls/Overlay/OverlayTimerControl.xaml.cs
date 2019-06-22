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
    /// Interaction logic for OverlayTimerControl.xaml
    /// </summary>
    public partial class OverlayTimerControl : OverlayItemControl
    {
        private OverlayTimerItemViewModel viewModel;

        public OverlayTimerControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayTimerItemViewModel();
        }

        public OverlayTimerControl(OverlayTimer item)
        {
            InitializeComponent();

            this.viewModel = new OverlayTimerItemViewModel(item);
        }

        public OverlayTimerControl(OverlayTimerItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayTimerItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTimerItemViewModel((OverlayTimer)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
        }

        public override void SetOverlayItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTimerItemViewModel((OverlayTimerItemModel)item);
            }
        }

        public override OverlayItemModelBase GetOverlayItem()
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
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(OverlayTimer.TimerCompleteCommandName)));
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
                this.viewModel.TimerCompleteCommand = null;
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.TimerCompleteCommand = (CustomCommand)e;
        }
    }
}
