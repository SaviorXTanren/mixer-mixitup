using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayImageItemControl.xaml
    /// </summary>
    public partial class OverlayImageItemControl : OverlayItemControl
    {
        private OverlayImageItemViewModel viewModel;

        public OverlayImageItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayImageItemViewModel();
        }

        public OverlayImageItemControl(OverlayImageItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayImageItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override void SetItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayImageItemViewModel((OverlayImageItemModel)item);
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
