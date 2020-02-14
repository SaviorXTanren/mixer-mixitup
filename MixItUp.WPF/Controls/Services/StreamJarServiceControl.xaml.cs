using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamJarServiceControl.xaml
    /// </summary>
    public partial class StreamJarServiceControl : ServiceControlBase
    {
        private StreamJarServiceControlViewModel viewModel;

        public StreamJarServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new StreamJarServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
