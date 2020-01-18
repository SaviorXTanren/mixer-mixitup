using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OverlayServiceControl.xaml
    /// </summary>
    public partial class OverlayServiceControl : ServiceControlBase
    {
        private OverlayServiceControlViewModel viewModel;

        public OverlayServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new OverlayServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
