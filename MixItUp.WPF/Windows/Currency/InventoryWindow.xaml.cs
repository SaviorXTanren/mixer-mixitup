using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Currency;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
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
            await this.viewModel.OnOpen();
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

        private void ShopBuyCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.ShopBuyCommand = command; };
            window.ForceShow();
        }

        private void ShopSellCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.ShopSellCommand = command; };
            window.ForceShow();
        }

        private void TradeCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { this.viewModel.TradeCommand = command; };
            window.ForceShow();
        }
    }
}
