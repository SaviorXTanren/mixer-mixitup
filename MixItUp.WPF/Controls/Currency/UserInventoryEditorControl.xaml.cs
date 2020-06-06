using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for UserInventoryEditorControl.xaml
    /// </summary>
    public partial class UserInventoryEditorControl : UserControl
    {
        private UserDataModel user;

        private UserInventoryModel inventory;
        private UserInventoryDataViewModel inventoryData;

        public UserInventoryEditorControl(UserDataModel user, UserInventoryModel inventory, UserInventoryDataViewModel inventoryData)
        {
            this.user = user;
            this.inventory = inventory;
            this.inventoryData = inventoryData;

            InitializeComponent();

            this.Loaded += UserInventoryEditorControl_Loaded;

            this.DataContext = this.inventory;
        }

        private void UserInventoryEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.InventoryItemsStackPanel.Children.Clear();
            foreach (UserInventoryItemModel item in this.inventory.Items.Values)
            {
                this.InventoryItemsStackPanel.Children.Add(new UserCurrencyIndividualEditorControl(this.user, item, this.inventoryData));
            }
        }
    }
}
