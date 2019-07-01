using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlaySongRequestsControl.xaml
    /// </summary>
    public partial class OverlaySongRequestsControl : OverlayItemControl
    {
        private OverlaySongRequestsItemViewModel viewModel;

        public OverlaySongRequestsControl()
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsItemViewModel();
        }

        public OverlaySongRequestsControl(OverlayItemModelBase item)
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsItemViewModel((OverlaySongRequestsListItemModel)item);
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlaySongRequestsItemViewModel((OverlaySongRequests)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
