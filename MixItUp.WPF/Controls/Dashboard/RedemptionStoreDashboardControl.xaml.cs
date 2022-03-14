using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for RedemptionStoreDashboardControl.xaml
    /// </summary>
    public partial class RedemptionStoreDashboardControl : DashboardControlBase
    {
        private RedemptionStoreMainControlViewModel viewModel;

        public RedemptionStoreDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new RedemptionStoreMainControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }
    }
}
