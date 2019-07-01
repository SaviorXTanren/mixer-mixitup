using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameQueueControl.xaml
    /// </summary>
    public partial class OverlayGameQueueControl : OverlayItemControl
    {
        private OverlayGameQueueItemViewModel viewModel;

        public OverlayGameQueueControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueItemViewModel();
        }

        public OverlayGameQueueControl(OverlayItemModelBase item)
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueItemViewModel((OverlayGameQueueListItemModel)item);
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayGameQueueItemViewModel((OverlayGameQueue)item);
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
