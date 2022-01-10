using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TreatStreamServiceControl.xaml
    /// </summary>
    public partial class TreatStreamServiceControl : ServiceControlBase
    {
        private TreatStreamServiceControlViewModel viewModel;

        public TreatStreamServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TreatStreamServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
