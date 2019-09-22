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
    /// Interaction logic for OverlayLeaderboardListItemControl.xaml
    /// </summary>
    public partial class OverlayLeaderboardListItemControl : OverlayItemControl
    {
        private OverlayLeaderboardListItemViewModel viewModel;
        
        public OverlayLeaderboardListItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayLeaderboardListItemViewModel();
        }

        public OverlayLeaderboardListItemControl(OverlayLeaderboardListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayLeaderboardListItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

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

        private void CreateNewLeaderCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand("New Leader")));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.NewLeaderCommand = (CustomCommand)e;
        }

        private void NewLeader_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(this.viewModel.NewLeaderCommand));
            window.Show();
        }

        private void NewLeaderCommand_DeleteClicked(object sender, RoutedEventArgs e)
        {
            this.viewModel.NewLeaderCommand = null;
        }
    }
}
