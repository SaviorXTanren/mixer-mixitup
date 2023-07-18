using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for InfiniteAlbumServiceControl.xaml
    /// </summary>
    public partial class InfiniteAlbumServiceControl : ServiceControlBase
    {
        private InfiniteAlbumServiceControlViewModel viewModel;

        public InfiniteAlbumServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new InfiniteAlbumServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
