using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamAvatarsServiceControl.xaml
    /// </summary>
    public partial class StreamAvatarsServiceControl : ServiceControlBase
    {
        private StreamAvatarsServiceControlViewModel viewModel;

        public StreamAvatarsServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamAvatarsServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
