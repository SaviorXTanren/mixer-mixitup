using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TiltifyServiceControl.xaml
    /// </summary>
    public partial class TiltifyServiceControl : ServiceControlBase
    {
        private TiltifyServiceControlViewModel viewModel;

        private string authorizationToken = null;
        private bool windowClosed = false;

        public TiltifyServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TiltifyServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
