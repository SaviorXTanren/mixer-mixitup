using MixItUp.Base.ViewModel.Window.Currency;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for RedemptionStoreWindow.xaml
    /// </summary>
    public partial class RedemptionStoreWindow : LoadingWindowBase
    {
        private RedemptionStoreWindowViewModel viewModel;

        public RedemptionStoreWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel = new RedemptionStoreWindowViewModel();
            await this.viewModel.OnLoaded();
            await base.OnLoaded();
        }

        private void CustomProductNewCommandButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CustomProductCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {

        }

        private void CustomProductCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ManualRedeemNeededCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {

        }

        private void DefaultRedeemCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {

        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
