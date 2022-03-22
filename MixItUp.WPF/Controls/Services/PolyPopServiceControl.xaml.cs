using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for PolyPopServiceControl.xaml
    /// </summary>
    public partial class PolyPopServiceControl : ServiceControlBase
    {
        private PolyPopServiceControlViewModel viewModel;

        public PolyPopServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new PolyPopServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
