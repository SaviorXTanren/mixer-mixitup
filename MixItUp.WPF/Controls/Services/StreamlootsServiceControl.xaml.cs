using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamlootsServiceControl.xaml
    /// </summary>
    public partial class StreamlootsServiceControl : ServiceControlBase
    {
        private StreamlootsServiceControlViewModel viewModel;

        public StreamlootsServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamlootsServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
