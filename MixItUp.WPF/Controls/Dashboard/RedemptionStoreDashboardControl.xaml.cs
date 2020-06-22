using MixItUp.Base.ViewModel.Controls.MainControls;
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
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ManualRedeemButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void RefundButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
