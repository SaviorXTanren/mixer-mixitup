using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for IFTTTServiceControl.xaml
    /// </summary>
    public partial class IFTTTServiceControl : ServiceControlBase
    {
        private IFTTTServiceControlViewModel viewModel;

        public IFTTTServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new IFTTTServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
