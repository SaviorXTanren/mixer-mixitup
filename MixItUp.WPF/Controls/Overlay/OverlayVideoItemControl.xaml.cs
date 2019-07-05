using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayVideoItemControl.xaml
    /// </summary>
    public partial class OverlayVideoItemControl : OverlayItemControl
    {
        private OverlayVideoItemViewModel viewModel;

        public OverlayVideoItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayVideoItemViewModel();
        }

        public OverlayVideoItemControl(OverlayVideoItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayVideoItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override void SetItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayVideoItemViewModel((OverlayVideoItemModel)item);
            }
        }

        public override OverlayItemModelBase GetItem()
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
