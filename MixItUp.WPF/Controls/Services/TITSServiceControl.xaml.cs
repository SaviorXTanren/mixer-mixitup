using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TITSServiceControl.xaml
    /// </summary>
    public partial class TITSServiceControl : ServiceControlBase
    {
        private TITSServiceControlViewModel viewModel;

        public TITSServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TITSServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
