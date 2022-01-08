using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for PatreonServiceControl.xaml
    /// </summary>
    public partial class PatreonServiceControl : ServiceControlBase
    {
        private PatreonServiceControlViewModel viewModel;

        public PatreonServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new PatreonServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
