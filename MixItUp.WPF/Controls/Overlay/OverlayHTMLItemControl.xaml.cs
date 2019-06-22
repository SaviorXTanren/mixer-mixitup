using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayHTMLItemControl.xaml
    /// </summary>
    public partial class OverlayHTMLItemControl : OverlayItemControl
    {
        private OverlayHTMLItemViewModel viewModel;

        public OverlayHTMLItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayHTMLItemViewModel();
        }

        public OverlayHTMLItemControl(OverlayHTMLItem item)
        {
            InitializeComponent();

            this.viewModel = new OverlayHTMLItemViewModel(item);
        }

        public OverlayHTMLItemControl(OverlayHTMLItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayHTMLItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayHTMLItemViewModel((OverlayHTMLItem)item);
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
                this.viewModel = new OverlayHTMLItemViewModel((OverlayHTMLItemModel)item);
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
