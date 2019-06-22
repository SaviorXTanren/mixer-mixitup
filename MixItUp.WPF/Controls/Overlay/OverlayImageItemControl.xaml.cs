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

        public OverlayImageItemControl(OverlayImageItem item)
        {
            InitializeComponent();

            this.viewModel = new OverlayImageItemViewModel(item);
        }

        public OverlayImageItemControl(OverlayImageItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayImageItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayImageItemViewModel((OverlayImageItem)item);
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
                this.viewModel = new OverlayImageItemViewModel((OverlayImageItemModel)item);
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
