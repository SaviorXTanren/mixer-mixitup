using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for UserInventoryEditorControl.xaml
    /// </summary>
    public partial class UserInventoryEditorControl : UserControl
    {
        private UserInventoryViewModel inventory;
        private UserInventoryDataViewModel inventoryData;

        public UserInventoryEditorControl(UserInventoryViewModel inventory, UserInventoryDataViewModel inventoryData)
        {
            this.inventory = inventory;
            this.inventoryData = inventoryData;

            InitializeComponent();

            this.Loaded += UserInventoryEditorControl_Loaded;

            this.DataContext = this.inventory;
        }

        private void UserInventoryEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.InventoryItemsStackPanel.Children.Clear();
            foreach (UserInventoryItemViewModel item in this.inventory.Items.Values)
            {
                this.InventoryItemsStackPanel.Children.Add(new UserCurrencyIndividualEditorControl(item, this.inventoryData));
            }
        }
    }
}
