using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayMixerClipControl.xaml
    /// </summary>
    public partial class OverlayMixerClipControl : OverlayItemControl
    {
        private OverlayMixerClipItemViewModel viewModel;

        public OverlayMixerClipControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayMixerClipItemViewModel();
        }

        public OverlayMixerClipControl(OverlayMixerClip item)
        {
            InitializeComponent();

            this.viewModel = new OverlayMixerClipItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayMixerClipItemViewModel((OverlayMixerClip)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
