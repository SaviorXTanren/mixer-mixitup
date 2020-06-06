using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for InventoryRequirementControl.xaml
    /// </summary>
    public partial class InventoryRequirementControl : LoadingControlBase
    {
        public InventoryRequirementControl()
        {
            InitializeComponent();
        }

        public InventoryModel GetInventoryType() { return (InventoryModel)this.InventoryTypeComboBox.SelectedItem; }

        public InventoryItemModel GetInventoryItem() { return (InventoryItemModel)this.InventoryItemComboBox.SelectedItem; }

        public int GetItemAmount()
        {
            if (int.TryParse(this.InventoryItemAmountTextBox.Text, out int amount) && amount > 0)
            {
                return amount;
            }
            return 0;
        }

        public InventoryRequirementViewModel GetInventoryRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetInventoryType() != null && this.GetInventoryItem() != null && this.GetItemAmount() > 0)
            {
                return new InventoryRequirementViewModel(this.GetInventoryType(), this.GetInventoryItem(), this.GetItemAmount());
            }
            return null;
        }

        public void SetInventoryRequirement(InventoryRequirementViewModel inventoryRequirement)
        {
            if (inventoryRequirement != null && ChannelSession.Settings.Inventory.ContainsKey(inventoryRequirement.InventoryID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.InventoryTypeComboBox.ItemsSource = ChannelSession.Settings.Inventory.Values;
                this.InventoryTypeComboBox.SelectedItem = ChannelSession.Settings.Inventory[inventoryRequirement.InventoryID];

                this.InventoryItemComboBox.IsEnabled = true;
                this.InventoryItemComboBox.ItemsSource = ChannelSession.Settings.Inventory[inventoryRequirement.InventoryID].Items.Values;

#pragma warning disable CS0612 // Type or member is obsolete
                if (!string.IsNullOrEmpty(inventoryRequirement.ItemName))
                {
                    InventoryItemModel item = ChannelSession.Settings.Inventory[inventoryRequirement.InventoryID].GetItem(inventoryRequirement.ItemName);
                    if (item != null)
                    {
                        inventoryRequirement.ItemID = item.ID;
                    }
                    inventoryRequirement.ItemName = null;
                }
#pragma warning restore CS0612 // Type or member is obsolete

                if (ChannelSession.Settings.Inventory[inventoryRequirement.InventoryID].ItemExists(inventoryRequirement.ItemID))
                {
                    this.InventoryItemComboBox.SelectedItem = ChannelSession.Settings.Inventory[inventoryRequirement.InventoryID].GetItem(inventoryRequirement.ItemID);
                }

                this.InventoryItemAmountTextBox.IsEnabled = true;
                this.InventoryItemAmountTextBox.Text = inventoryRequirement.Amount.ToString();
            }
        }

        public async Task<bool> Validate()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault())
            {
                if (this.GetInventoryType() == null)
                {
                    await DialogHelper.ShowMessage("An Inventory must be specified when an Inventory requirement is set");
                    return false;
                }

                if (this.GetInventoryItem() == null)
                {
                    await DialogHelper.ShowMessage("An item must be specified when an Inventory requirement is set");
                    return false;
                }

                if (this.GetItemAmount() <= 0)
                {
                    await DialogHelper.ShowMessage("A valid item amount must be specified when an Inventory requirement is set");
                    return false;
                }
            }
            return true;
        }

        protected override Task OnLoaded()
        {
            InventoryRequirementViewModel requirement = this.GetInventoryRequirement();

            if (ChannelSession.Settings.Inventory.Count > 0)
            {
                this.EnableDisableToggleSwitch.IsEnabled = true;
                this.InventoryTypeComboBox.ItemsSource = ChannelSession.Settings.Inventory.Values;
            }

            this.SetInventoryRequirement(requirement);

            return Task.FromResult(0);
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.OnLoaded();
        }

        private void EnableDisableToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.InventoryDataGrid.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
        }

        private void InventoryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InventoryModel inventory = this.GetInventoryType();

            this.InventoryItemComboBox.IsEnabled = true;
            this.InventoryItemComboBox.ItemsSource = inventory.Items.Values;

            this.InventoryItemAmountTextBox.IsEnabled = true;
        }
    }
}
