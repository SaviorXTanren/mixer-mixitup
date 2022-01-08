using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for XSplitServiceControl.xaml
    /// </summary>
    public partial class XSplitServiceControl : ServiceControlBase
    {
        private XSplitServiceControlViewModel viewModel;

        public XSplitServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new XSplitServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
