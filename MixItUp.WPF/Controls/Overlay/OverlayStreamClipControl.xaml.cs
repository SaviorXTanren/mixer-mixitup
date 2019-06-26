using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayStreamClipControl.xaml
    /// </summary>
    public partial class OverlayStreamClipControl : OverlayItemControl
    {
        private OverlayStreamClipItemViewModel viewModel;

        public OverlayStreamClipControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayStreamClipItemViewModel();
        }

        public OverlayStreamClipControl(OverlayStreamClipItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayStreamClipItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayStreamClipItemViewModel((OverlayMixerClip)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
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
