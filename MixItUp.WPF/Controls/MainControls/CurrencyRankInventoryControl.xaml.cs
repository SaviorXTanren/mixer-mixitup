using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Windows.Currency;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CurrencyRankInventoryControl.xaml
    /// </summary>
    public partial class CurrencyRankInventoryControl : MainControlBase
    {
        private CurrencyRankInventoryMainControlViewModel viewModel;

        public CurrencyRankInventoryControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CurrencyRankInventoryMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrencyRankInventoryContainerViewModel item = (CurrencyRankInventoryContainerViewModel)button.DataContext;
            if (item.Inventory != null)
            {
                InventoryWindow window = new InventoryWindow(item.Inventory);
                window.Closed += Window_Closed;
                window.Show();
            }
            else
            {
                CurrencyWindow window = new CurrencyWindow(item.Currency);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrencyRankInventoryContainerViewModel item = (CurrencyRankInventoryContainerViewModel)button.DataContext;
            this.viewModel.DeleteItem(item);
        }

        private void AddNewCurrencyRankButton_Click(object sender, RoutedEventArgs e)
        {
            CurrencyWindow window = new CurrencyWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void AddNewInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            InventoryWindow window = new InventoryWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshList();
        }
    }
}
