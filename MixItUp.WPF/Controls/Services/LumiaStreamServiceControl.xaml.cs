using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for LumiaStreamServiceControl.xaml
    /// </summary>
    public partial class LumiaStreamServiceControl : ServiceControlBase
    {
        private LumiaStreamServiceControlViewModel viewModel;

        public LumiaStreamServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new LumiaStreamServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
