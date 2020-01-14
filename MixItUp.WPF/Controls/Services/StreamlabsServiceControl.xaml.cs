using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamlabsServiceControl.xaml
    /// </summary>
    public partial class StreamlabsServiceControl : ServiceControlBase
    {
        private StreamlabsServiceControlViewModel viewModel;

        public StreamlabsServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamlabsServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
