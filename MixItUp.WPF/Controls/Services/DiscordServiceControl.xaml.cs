using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for DiscordServiceControl.xaml
    /// </summary>
    public partial class DiscordServiceControl : ServiceControlBase
    {
        private DiscordServiceControlViewModel viewModel;

        public DiscordServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new DiscordServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
