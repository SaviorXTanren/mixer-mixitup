using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Window.Currency;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for InventoryWindow.xaml
    /// </summary>
    public partial class InventoryWindow : LoadingWindowBase
    {
        private InventoryWindowViewModel viewModel;

        public InventoryWindow()
        {
            this.viewModel = new InventoryWindowViewModel();

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public InventoryWindow(InventoryModel inventory)
        {
            this.viewModel = new InventoryWindowViewModel(inventory);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();

            this.ShopBuyCommandButtonsControl.DataContext = this.viewModel.ShopBuyCommand;
            this.ShopSellCommandButtonsControl.DataContext = this.viewModel.ShopSellCommand;
            this.TradeCommandButtonsControl.DataContext = this.viewModel.TradeCommand;

            await base.OnLoaded();
        }

        private void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.viewModel.SelectedItem = (InventoryItemModel)button.DataContext;
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InventoryItemModel item = (InventoryItemModel)button.DataContext;
            this.viewModel.Items.Remove(item);
            this.viewModel.SelectedItem = null;
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    await this.viewModel.Save();
                    this.Close();
                }
            });
        }

        protected void CommandButtonsControl_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }
    }
}
