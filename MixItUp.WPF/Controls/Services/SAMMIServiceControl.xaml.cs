using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for SAMMIServiceControl.xaml
    /// </summary>
    public partial class SAMMIServiceControl : ServiceControlBase
    {
        private SAMMIServiceControlViewModel viewModel;

        public SAMMIServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new SAMMIServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}