using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
        private UserInventoryViewModel inventory;

        private ObservableCollection<UserInventoryItemViewModel> items = new ObservableCollection<UserInventoryItemViewModel>();

        private Dictionary<UserDataViewModel, int> userImportData = new Dictionary<UserDataViewModel, int>();

        public InventoryWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public InventoryWindow(UserInventoryViewModel inventory)
        {
            this.inventory = inventory;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.ItemsListView.ItemsSource = this.items;

            this.DefaultMaxAmountTextBox.Text = "99";
            this.AutomaticResetComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyResetRateEnum>();

            this.AutomaticResetComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyResetRateEnum.Never);

            if (this.inventory != null)
            {
                this.NameTextBox.Text = this.inventory.Name;

                this.DefaultMaxAmountTextBox.Text = this.inventory.DefaultMaxAmount.ToString();

                this.AutomaticResetComboBox.SelectedItem = EnumHelper.GetEnumName(this.inventory.ResetInterval);

                foreach (UserInventoryItemViewModel item in this.inventory.Items.Values)
                {
                    this.items.Add(item);
                }
            }
            await base.OnLoaded();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency-&-Rank");
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserInventoryItemViewModel item = (UserInventoryItemViewModel)button.DataContext;
            this.items.Remove(item);
        }

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.ItemNameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("You must specify a name for the item");
                    return;
                }

                int maxAmount = -1;
                if (!string.IsNullOrEmpty(this.ItemMaxAmountTextBox.Text) && (!int.TryParse(this.ItemMaxAmountTextBox.Text, out maxAmount) || maxAmount <= 0))
                {
                    await MessageBoxHelper.ShowMessageDialog("The item max amount must be either blank or a number greater than 0");
                    return;
                }

                UserInventoryItemViewModel existingItem = this.items.FirstOrDefault(i => i.Name.Equals(this.ItemNameTextBox.Text, StringComparison.CurrentCultureIgnoreCase));
                if (existingItem != null)
                {
                    this.items.Remove(existingItem);
                }

                UserInventoryItemViewModel item = new UserInventoryItemViewModel(this.ItemNameTextBox.Text, maxAmount);
                this.items.Add(item);

                this.ItemNameTextBox.Text = string.Empty;
                this.ItemMaxAmountTextBox.Text = string.Empty;
            });
        }

        private async void ManualResetButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog(string.Format("Do you want to reset all items?")))
                {
                    if (this.inventory != null)
                    {
                        await this.inventory.Reset();
                    }
                }
            });
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.NameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("An inventory name must be specified");
                    return;
                }

                UserInventoryViewModel dupeInventory = ChannelSession.Settings.Inventories.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeInventory != null && (this.inventory == null || !this.inventory.ID.Equals(dupeInventory.ID)))
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists an inventory with this name");
                    return;
                }

                UserCurrencyViewModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeCurrency != null)
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists a currency or rank system with this name");
                    return;
                }

                if (string.IsNullOrEmpty(this.DefaultMaxAmountTextBox.Text) || !int.TryParse(this.DefaultMaxAmountTextBox.Text, out int maxAmount) || maxAmount <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The default max amount must be greater than 0");
                    return;
                }

                if (this.items.Count == 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("At least 1 item must be added");
                    return;
                }

                if (this.inventory == null)
                {
                    this.inventory = new UserInventoryViewModel();
                    ChannelSession.Settings.Inventories[this.inventory.ID] = this.inventory;
                }

                this.inventory.Name = this.NameTextBox.Text;
                this.inventory.DefaultMaxAmount = maxAmount;
                this.inventory.ResetInterval = EnumHelper.GetEnumValueFromString<CurrencyResetRateEnum>((string)this.AutomaticResetComboBox.SelectedItem);
                this.inventory.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.inventory.Name);
                this.inventory.Items = new Dictionary<string, UserInventoryItemViewModel>(this.items.ToDictionary(i => i.Name, i => i));

                await ChannelSession.SaveSettings();

                this.Close();
            });
        }
    }
}
