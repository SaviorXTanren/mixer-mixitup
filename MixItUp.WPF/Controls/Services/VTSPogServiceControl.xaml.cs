using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for VTSPogServiceControl.xaml
    /// </summary>
    public partial class VTSPogServiceControl : ServiceControlBase
    {
        private VTSPogServiceControlViewModel viewModel;

        public VTSPogServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new VTSPogServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
