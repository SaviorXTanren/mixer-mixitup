using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OvrStreamServiceControl.xaml
    /// </summary>
    public partial class OvrStreamServiceControl : ServiceControlBase
    {
        private OvrStreamServiceControlViewModel viewModel;

        public OvrStreamServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new OvrStreamServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}