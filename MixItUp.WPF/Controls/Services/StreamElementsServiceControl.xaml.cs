using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamElementsServiceControl.xaml
    /// </summary>
    public partial class StreamElementsServiceControl : ServiceControlBase
    {
        private StreamElementsServiceControlViewModel viewModel;

        public StreamElementsServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamElementsServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}