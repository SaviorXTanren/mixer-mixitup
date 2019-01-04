using MixItUp.Base;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
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

        public UserInventoryViewModel GetInventoryType() { return (UserInventoryViewModel)this.InventoryTypeComboBox.SelectedItem; }

        public UserInventoryItemViewModel GetInventoryItem() { return (UserInventoryItemViewModel)this.InventoryItemComboBox.SelectedItem; }

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
            if (inventoryRequirement != null && ChannelSession.Settings.Inventories.ContainsKey(inventoryRequirement.InventoryID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.InventoryTypeComboBox.ItemsSource = ChannelSession.Settings.Inventories.Values;
                this.InventoryTypeComboBox.SelectedItem = ChannelSession.Settings.Inventories[inventoryRequirement.InventoryID];

                this.InventoryItemComboBox.IsEnabled = true;
                this.InventoryItemComboBox.ItemsSource = ChannelSession.Settings.Inventories[inventoryRequirement.InventoryID].Items.Values;
                if (ChannelSession.Settings.Inventories[inventoryRequirement.InventoryID].Items.ContainsKey(inventoryRequirement.ItemName))
                {
                    this.InventoryItemComboBox.SelectedItem = ChannelSession.Settings.Inventories[inventoryRequirement.InventoryID].Items[inventoryRequirement.ItemName];
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
                    await MessageBoxHelper.ShowMessageDialog("An Inventory must be specified when an Inventory requirement is set");
                    return false;
                }

                if (this.GetInventoryItem() == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("An item must be specified when an Inventory requirement is set");
                    return false;
                }

                if (this.GetItemAmount() <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid item amount must be specified when an Inventory requirement is set");
                    return false;
                }
            }
            return true;
        }

        protected override Task OnLoaded()
        {
            InventoryRequirementViewModel requirement = this.GetInventoryRequirement();

            if (ChannelSession.Settings.Inventories.Count > 0)
            {
                this.EnableDisableToggleSwitch.IsEnabled = true;
                this.InventoryTypeComboBox.ItemsSource = ChannelSession.Settings.Inventories.Values;
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
            UserInventoryViewModel inventory = this.GetInventoryType();

            this.InventoryItemComboBox.IsEnabled = true;
            this.InventoryItemComboBox.ItemsSource = inventory.Items.Values;

            this.InventoryItemAmountTextBox.IsEnabled = true;
        }
    }
}
