using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamlabsOBSServiceControl.xaml
    /// </summary>
    public partial class StreamlabsOBSServiceControl : ServiceControlBase
    {
        private StreamlabsOBSServiceControlViewModel viewModel;

        public StreamlabsOBSServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamlabsOBSServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
