using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemPositionControl.xaml
    /// </summary>
    public partial class OverlayItemPositionControl : LoadingControlBase
    {
        private OverlayItemPositionViewModel viewModel;

        public OverlayItemPositionControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayItemPositionViewModel();
        }

        public OverlayItemPositionControl(OverlayItemPositionModel position)
        {
            InitializeComponent();

            this.viewModel = new OverlayItemPositionViewModel(position);
        }

        public void SetPosition(OverlayItemPositionModel position)
        {
            this.viewModel.SetPosition(position);
        }

        public OverlayItemPositionModel GetPosition()
        {
            return this.viewModel.GetPosition();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
