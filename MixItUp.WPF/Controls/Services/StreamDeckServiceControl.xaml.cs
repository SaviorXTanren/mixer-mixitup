using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    public partial class StreamDeckServiceControl : ServiceControlBase
    {
        private StreamDeckServiceControlViewModel viewModel;

        public StreamDeckServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamDeckServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
