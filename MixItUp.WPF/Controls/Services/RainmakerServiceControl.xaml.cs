using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for RainmakerServiceControl.xaml
    /// </summary>
    public partial class RainmakerServiceControl : ServiceControlBase
    {
        private RainmakerServiceControlViewModel viewModel;

        public RainmakerServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new RainmakerServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
