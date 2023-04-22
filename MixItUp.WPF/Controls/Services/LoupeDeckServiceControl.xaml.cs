using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    public partial class LoupeDeckServiceControl : ServiceControlBase
    {
        private LoupeDeckServiceControlViewModel viewModel;

        public LoupeDeckServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new LoupeDeckServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
