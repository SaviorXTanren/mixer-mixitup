using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

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
    }
}
