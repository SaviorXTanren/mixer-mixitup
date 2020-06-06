using MixItUp.Base.Model.Currency;
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
        private InventoryModel inventory;

        public UserInventoryEditorControl(UserDataModel user, InventoryModel inventory)
        {
            this.user = user;
            this.inventory = inventory;

            InitializeComponent();

            this.Loaded += UserInventoryEditorControl_Loaded;

            this.DataContext = this.inventory;
        }

        private void UserInventoryEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.InventoryItemsStackPanel.Children.Clear();
            foreach (InventoryItemModel item in this.inventory.Items.Values)
            {
                this.InventoryItemsStackPanel.Children.Add(new UserCurrencyIndividualEditorControl(this.user, this.inventory, item));
            }
        }
    }
}
