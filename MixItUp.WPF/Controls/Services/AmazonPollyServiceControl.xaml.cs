using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for AmazonPollyServiceControl.xaml
    /// </summary>
    public partial class AmazonPollyServiceControl : ServiceControlBase
    {
        private AmazonPollyServiceControlViewModel viewModel;

        public AmazonPollyServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new AmazonPollyServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
