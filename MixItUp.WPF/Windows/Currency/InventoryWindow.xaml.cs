using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public const string ItemsBoughtCommandName = "Shop Items Bought";
        public const string ItemsSoldCommandName = "Shop Items Sold";
        public const string ItemsTradedCommandName = "Items Traded";

        private UserInventoryModel inventory;

        private ObservableCollection<UserInventoryItemModel> items = new ObservableCollection<UserInventoryItemModel>();
        private UserInventoryItemModel selectedItem;

        private Dictionary<UserDataModel, int> userImportData = new Dictionary<UserDataModel, int>();

        public InventoryWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public InventoryWindow(UserInventoryModel inventory)
        {
            this.inventory = inventory;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.ItemsListView.ItemsSource = this.items;

            this.DefaultMaxAmountTextBox.Text = "99";
            this.AutomaticResetComboBox.ItemsSource = Enum.GetValues(typeof(CurrencyResetRateEnum));

            this.ShopCurrencyComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;

            this.ShopCommandTextBox.Text = "!shop";

            CustomCommand buyCommand = new CustomCommand(InventoryWindow.ItemsBoughtCommandName);
            buyCommand.Actions.Add(new ChatAction("You bought $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false, isWhisper: true));
            this.ShopItemsBoughtCommandButtonsControl.DataContext = buyCommand;

            CustomCommand sellCommand = new CustomCommand(InventoryWindow.ItemsSoldCommandName);
            sellCommand.Actions.Add(new ChatAction("You sold $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false, isWhisper: true));
            this.ShopItemsSoldCommandButtonsControl.DataContext = sellCommand;

            this.TradeCommandTextBox.Text = "!trade";

            CustomCommand tradeCommand = new CustomCommand(InventoryWindow.ItemsTradedCommandName);
            tradeCommand.Actions.Add(new ChatAction("@$username traded $itemtotal $itemname to @$targetusername for $targetitemtotal $targetitemname", sendAsStreamer: false));
            this.TradeItemsBoughtCommandButtonsControl.DataContext = tradeCommand;

            this.AutomaticResetComboBox.SelectedItem = CurrencyResetRateEnum.Never;

            if (this.inventory != null)
            {
                this.NameTextBox.Text = this.inventory.Name;

                this.DefaultMaxAmountTextBox.Text = this.inventory.DefaultMaxAmount.ToString();

                this.AutomaticResetComboBox.SelectedItem = this.inventory.ResetInterval;

                foreach (UserInventoryItemModel item in this.inventory.Items.Values)
                {
                    this.items.Add(item);
                }

                this.ShopEnableDisableToggleButton.IsChecked = this.inventory.ShopEnabled;
                this.ShopCommandTextBox.Text = this.inventory.ShopCommand;
                if (ChannelSession.Settings.Currencies.ContainsKey(this.inventory.ShopCurrencyID))
                {
                    this.ShopCurrencyComboBox.SelectedItem = ChannelSession.Settings.Currencies[this.inventory.ShopCurrencyID];
                }
                this.ShopItemsBoughtCommandButtonsControl.DataContext = this.inventory.ItemsBoughtCommand;
                this.ShopItemsSoldCommandButtonsControl.DataContext = this.inventory.ItemsSoldCommand;

                this.TradeEnableDisableToggleButton.IsChecked = this.inventory.TradeEnabled;
                this.TradeCommandTextBox.Text = this.inventory.TradeCommand;
                this.TradeItemsBoughtCommandButtonsControl.DataContext = this.inventory.ItemsTradedCommand;
            }

            await base.OnLoaded();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency-&-Rank");
        }

        private void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.selectedItem = (UserInventoryItemModel)button.DataContext;

            this.ItemNameTextBox.Text = this.selectedItem.Name;
            this.ItemMaxAmountTextBox.Text = this.selectedItem.HasMaxAmount ? this.selectedItem.MaxAmount.ToString() : string.Empty;
            this.ItemBuyAmountTextBox.Text = this.selectedItem.BuyAmountString;
            this.ItemSellAmountTextBox.Text = this.selectedItem.SellAmountString;
            this.AddItemButton.Content = MixItUp.Base.Resources.Update;
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserInventoryItemModel item = (UserInventoryItemModel)button.DataContext;
            this.items.Remove(item);
            this.selectedItem = null;
        }

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.ItemNameTextBox.Text))
                {
                    await DialogHelper.ShowMessage("You must specify a name for the item");
                    return;
                }

                int maxAmount = -1;
                if (!string.IsNullOrEmpty(this.ItemMaxAmountTextBox.Text) && (!int.TryParse(this.ItemMaxAmountTextBox.Text, out maxAmount) || maxAmount <= 0))
                {
                    await DialogHelper.ShowMessage("The item max amount must be either blank or a number greater than 0");
                    return;
                }

                int buyAmount = -1;
                if (!string.IsNullOrEmpty(this.ItemBuyAmountTextBox.Text) && (!int.TryParse(this.ItemBuyAmountTextBox.Text, out buyAmount) || buyAmount < 0))
                {
                    await DialogHelper.ShowMessage("The item buy amount must be either blank or a number greater than 0");
                    return;
                }

                int sellAmount = -1;
                if (!string.IsNullOrEmpty(this.ItemSellAmountTextBox.Text) && (!int.TryParse(this.ItemSellAmountTextBox.Text, out sellAmount) || sellAmount < 0))
                {
                    await DialogHelper.ShowMessage("The item sell amount must be either blank or a number greater than 0");
                    return;
                }

                UserInventoryItemModel existingItem = this.items.FirstOrDefault(i => i.Name.Equals(this.ItemNameTextBox.Text, StringComparison.CurrentCultureIgnoreCase));
                if (existingItem != null)
                {
                    this.items.Remove(existingItem);
                }

                UserInventoryItemModel item = new UserInventoryItemModel(this.ItemNameTextBox.Text, maxAmount: maxAmount, buyAmount: buyAmount, sellAmount: sellAmount);
                if (this.selectedItem != null)
                {
                    item.ID = this.selectedItem.ID;
                    this.selectedItem = null;
                }
                this.items.Add(item);

                this.ItemNameTextBox.Text = string.Empty;
                this.ItemMaxAmountTextBox.Text = string.Empty;
                this.ItemBuyAmountTextBox.Text = string.Empty;
                this.ItemSellAmountTextBox.Text = string.Empty;
                this.AddItemButton.Content = MixItUp.Base.Resources.AddItem;
            });
        }

        private async void ManualResetButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format("Do you want to reset all items?")))
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
                    await DialogHelper.ShowMessage("An inventory name must be specified");
                    return;
                }

                UserInventoryModel dupeInventory = ChannelSession.Settings.Inventories.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeInventory != null && (this.inventory == null || !this.inventory.ID.Equals(dupeInventory.ID)))
                {
                    await DialogHelper.ShowMessage("There already exists an inventory with this name");
                    return;
                }

                UserCurrencyModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeCurrency != null)
                {
                    await DialogHelper.ShowMessage("There already exists a currency or rank system with this name");
                    return;
                }

                if (string.IsNullOrEmpty(this.DefaultMaxAmountTextBox.Text) || !int.TryParse(this.DefaultMaxAmountTextBox.Text, out int maxAmount) || maxAmount <= 0)
                {
                    await DialogHelper.ShowMessage("The default max amount must be greater than 0");
                    return;
                }

                if (this.items.Count == 0)
                {
                    await DialogHelper.ShowMessage("At least 1 item must be added");
                    return;
                }

                if (this.ShopEnableDisableToggleButton.IsChecked.GetValueOrDefault())
                {
                    if (string.IsNullOrEmpty(this.ShopCommandTextBox.Text))
                    {
                        await DialogHelper.ShowMessage("A command name must be specified for the shop");
                        return;
                    }

                    if (this.ShopCurrencyComboBox.SelectedIndex < 0)
                    {
                        await DialogHelper.ShowMessage("A currency must be specified for the shop");
                        return;
                    }
                }

                if (this.TradeEnableDisableToggleButton.IsChecked.GetValueOrDefault())
                {
                    if (string.IsNullOrEmpty(this.TradeCommandTextBox.Text))
                    {
                        await DialogHelper.ShowMessage("A command name must be specified for trading");
                        return;
                    }
                }

                if (this.inventory == null)
                {
                    this.inventory = new UserInventoryModel();
                    ChannelSession.Settings.Inventories[this.inventory.ID] = this.inventory;
                }

                this.inventory.Name = this.NameTextBox.Text;
                this.inventory.DefaultMaxAmount = maxAmount;
                this.inventory.ResetInterval = (CurrencyResetRateEnum)this.AutomaticResetComboBox.SelectedItem;
                this.inventory.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.inventory.Name);
                this.inventory.Items = new Dictionary<string, UserInventoryItemModel>(this.items.ToDictionary(i => i.Name, i => i));
                this.inventory.ShopEnabled = this.ShopEnableDisableToggleButton.IsChecked.GetValueOrDefault();
                this.inventory.ShopCommand = this.ShopCommandTextBox.Text;
                if (this.ShopCurrencyComboBox.SelectedIndex >= 0)
                {
                    UserCurrencyModel currency = (UserCurrencyModel)this.ShopCurrencyComboBox.SelectedItem;
                    if (currency != null)
                    {
                        this.inventory.ShopCurrencyID = currency.ID;
                    }
                }
                this.inventory.ItemsBoughtCommand = (CustomCommand)this.ShopItemsBoughtCommandButtonsControl.DataContext;
                this.inventory.ItemsSoldCommand = (CustomCommand)this.ShopItemsSoldCommandButtonsControl.DataContext;

                this.inventory.TradeEnabled = this.TradeEnableDisableToggleButton.IsChecked.GetValueOrDefault();
                this.inventory.TradeCommand = this.TradeCommandTextBox.Text;
                this.inventory.ItemsTradedCommand = (CustomCommand)this.TradeItemsBoughtCommandButtonsControl.DataContext;

                await ChannelSession.SaveSettings();

                this.Close();
            });
        }

        protected void ShopItemsCommandButtonsControl_EditClicked(object sender, RoutedEventArgs e)
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
