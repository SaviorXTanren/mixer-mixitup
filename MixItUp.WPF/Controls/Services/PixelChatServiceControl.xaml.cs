using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for PixelChatServiceControl.xaml
    /// </summary>
    public partial class PixelChatServiceControl : ServiceControlBase
    {
        private PixelChatServiceControlViewModel viewModel;

        public PixelChatServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new PixelChatServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
