using MixItUp.WPF.Windows.Currency;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for RedemptionStoreControl.xaml
    /// </summary>
    public partial class RedemptionStoreControl : MainControlBase
    {
        public RedemptionStoreControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            //this.DataContext = this.viewModel = new StreamPassMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            //await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            //await this.viewModel.OnVisible();
        }

        private void EditProducts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RedemptionStoreWindow window = new RedemptionStoreWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {

        }
    }
}
