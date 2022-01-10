using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for DeveloperAPIServiceControl.xaml
    /// </summary>
    public partial class DeveloperAPIServiceControl : ServiceControlBase
    {
        private DeveloperAPIServiceControlViewModel viewModel;

        public DeveloperAPIServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new DeveloperAPIServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
