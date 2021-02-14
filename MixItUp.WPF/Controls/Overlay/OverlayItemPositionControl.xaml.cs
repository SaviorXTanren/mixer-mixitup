using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
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
            if (this.DataContext is OverlayItemPositionViewModel)
            {
                this.viewModel = (OverlayItemPositionViewModel)this.DataContext;
            }
            else
            {
                this.DataContext = this.viewModel;
            }
            await this.viewModel.OnLoaded();
        }
    }
}
