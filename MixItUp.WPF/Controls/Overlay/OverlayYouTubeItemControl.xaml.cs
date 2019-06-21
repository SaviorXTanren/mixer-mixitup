using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayYouTubeItemControl.xaml
    /// </summary>
    public partial class OverlayYouTubeItemControl : OverlayItemControl
    {
        private OverlayYouTubeItemViewModel viewModel;

        public OverlayYouTubeItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayYouTubeItemViewModel();
        }

        public OverlayYouTubeItemControl(OverlayYouTubeItem item)
        {
            InitializeComponent();

            this.viewModel = new OverlayYouTubeItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayYouTubeItemViewModel((OverlayYouTubeItem)item);
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
                this.viewModel = new OverlayYouTubeItemViewModel((OverlayYouTubeItemModel)item);
            }
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
