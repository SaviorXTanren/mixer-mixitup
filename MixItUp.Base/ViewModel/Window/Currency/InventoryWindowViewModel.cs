using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Currency
{
    public class InventoryWindowViewModel : WindowViewModelBase
    {
        public const string ItemsBoughtCommandName = "Shop Items Bought";
        public const string ItemsSoldCommandName = "Shop Items Sold";
        public const string ItemsTradedCommandName = "Items Traded";

        public UserInventoryModel Inventory
        {
            get { return this.inventory; }
            set
            {
                this.inventory = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserInventoryModel inventory;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;
        public int DefaultItemMaxAmount
        {
            get { return this.defaultItemMaxAmount; }
            set
            {
                this.defaultItemMaxAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int defaultItemMaxAmount;

        public ObservableCollection<UserInventoryItemModel> Items { get; private set; } = new ObservableCollection<UserInventoryItemModel>();

        public UserInventoryItemModel SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("SaveItemButtonText");

                if (this.SelectedItem == null)
                {
                    this.ItemName = null;
                    this.ItemMaxAmount = 0;
                    this.ItemBuyAmount = 0;
                    this.ItemSellAmount = 0;
                }
                else
                {
                    this.ItemName = this.SelectedItem.Name;
                    this.ItemMaxAmount = this.SelectedItem.MaxAmount;
                    this.ItemBuyAmount = this.SelectedItem.BuyAmount;
                    this.ItemSellAmount = this.SelectedItem.SellAmount;
                }
            }
        }
        private UserInventoryItemModel selectedItem;
        public string ItemName
        {
            get { return this.itemName; }
            set
            {
                this.itemName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemName;
        public int ItemMaxAmount
        {
            get { return this.itemMaxAmount; }
            set
            {
                this.itemMaxAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int itemMaxAmount;
        public int ItemBuyAmount
        {
            get { return this.itemBuyAmount; }
            set
            {
                this.itemBuyAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int itemBuyAmount;
        public int ItemSellAmount
        {
            get { return this.itemSellAmount; }
            set
            {
                this.itemSellAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int itemSellAmount;
        public string SaveItemButtonText { get { return (this.SelectedItem != null) ? MixItUp.Base.Resources.Update : MixItUp.Base.Resources.AddItem; } }
        public ICommand SaveItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }

        public ICommand ManualResetCommand { get; private set; }

        public bool ShopEnabled
        {
            get { return this.shopEnabled; }
            set
            {
                this.shopEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool shopEnabled;
        public string ShopCommandText
        {
            get { return this.shopCommandText; }
            set
            {
                this.shopCommandText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string shopCommandText;
        public CustomCommand ShopBuyCommand
        {
            get { return this.shopBuyCommand; }
            set
            {
                this.shopBuyCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommand shopBuyCommand;
        public CustomCommand ShopSellCommand
        {
            get { return this.shopSellCommand; }
            set
            {
                this.shopSellCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommand shopSellCommand;

        public bool TradeEnabled
        {
            get { return this.tradeEnabled; }
            set
            {
                this.tradeEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool tradeEnabled;
        public string TradeCommandText
        {
            get { return this.tradeCommandText; }
            set
            {
                this.tradeCommandText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string tradeCommandText;
        public CustomCommand TradeCommand
        {
            get { return this.tradeCommand; }
            set
            {
                this.tradeCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommand tradeCommand;

        public IEnumerable<UserCurrencyModel> Currencies { get { return ChannelSession.Settings.Currencies.Values; } }
        public UserCurrencyModel SelectedShopCurrency
        {
            get { return this.selectedShopCurrency; }
            set
            {
                this.selectedShopCurrency = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserCurrencyModel selectedShopCurrency;

        public ICommand HelpCommand { get; private set; }

        private Dictionary<UserDataModel, int> userImportData = new Dictionary<UserDataModel, int>();

        public InventoryWindowViewModel(UserInventoryModel inventory)
            : this()
        {
            this.Inventory = inventory;

            this.Name = this.Inventory.Name;
            this.DefaultItemMaxAmount = this.Inventory.DefaultMaxAmount;

            foreach (UserInventoryItemModel item in this.Inventory.Items.Values)
            {
                this.Items.Add(item);
            }

            this.ShopEnabled = this.Inventory.ShopEnabled;
            this.ShopCommandText = this.Inventory.ShopCommand;
            this.SelectedShopCurrency = this.Currencies.FirstOrDefault(c => c.ID.Equals(this.Inventory.ShopCurrencyID));
            this.ShopBuyCommand = this.Inventory.ItemsBoughtCommand;
            this.ShopSellCommand = this.Inventory.ItemsSoldCommand;

            this.TradeEnabled = this.Inventory.TradeEnabled;
            this.TradeCommandText = this.Inventory.TradeCommand;
            this.TradeCommand = this.Inventory.ItemsTradedCommand;
        }

        public InventoryWindowViewModel()
        {
            this.DefaultItemMaxAmount = 99;

            this.ShopCommandText = "!shop";
            CustomCommand buyCommand = new CustomCommand(InventoryWindowViewModel.ItemsBoughtCommandName);
            buyCommand.Actions.Add(new ChatAction("You bought $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false, isWhisper: true));
            this.ShopBuyCommand = buyCommand;
            CustomCommand sellCommand = new CustomCommand(InventoryWindowViewModel.ItemsSoldCommandName);
            sellCommand.Actions.Add(new ChatAction("You sold $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false, isWhisper: true));
            this.ShopSellCommand = sellCommand;

            this.TradeCommandText = "!trade";
            CustomCommand tradeCommand = new CustomCommand(InventoryWindowViewModel.ItemsTradedCommandName);
            tradeCommand.Actions.Add(new ChatAction("@$username traded $itemtotal $itemname to @$targetusername for $targetitemtotal $targetitemname", sendAsStreamer: false));
            this.TradeCommand = tradeCommand;

            this.SaveItemCommand = this.CreateCommand(async (parameter) =>
            {
                if (string.IsNullOrEmpty(this.ItemName))
                {
                    await DialogHelper.ShowMessage("You must specify a name for the item");
                    return;
                }

                if (this.ItemMaxAmount < 0)
                {
                    await DialogHelper.ShowMessage("The item max amount must be either blank or a number greater than 0");
                    return;
                }

                if (this.ItemBuyAmount < 0)
                {
                    await DialogHelper.ShowMessage("The item buy amount must be either blank or a number greater than 0");
                    return;
                }

                if (this.ItemSellAmount < 0)
                {
                    await DialogHelper.ShowMessage("The item sell amount must be either blank or a number greater than 0");
                    return;
                }

                if (this.SelectedItem == null)
                {
                    UserInventoryItemModel existingItem = this.Items.FirstOrDefault(i => i.Name.Equals(this.ItemName, StringComparison.CurrentCultureIgnoreCase));
                    if (existingItem != null)
                    {
                        await DialogHelper.ShowMessage("An item with the same name already exists");
                        return;
                    }

                    this.Items.Add(new UserInventoryItemModel(this.ItemName, maxAmount: this.ItemMaxAmount, buyAmount: this.ItemBuyAmount, sellAmount: this.ItemSellAmount));
                }
                else
                {
                    this.SelectedItem.Name = name;
                    this.SelectedItem.MaxAmount = this.ItemMaxAmount;
                    this.SelectedItem.BuyAmount = this.ItemBuyAmount;
                    this.SelectedItem.SellAmount = this.ItemSellAmount;
                }

                this.SelectedItem = null;
            });

            this.ManualResetCommand = this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation("Do you want to reset all item amounts?"))
                {
                    if (this.Inventory != null)
                    {
                        await this.Inventory.Reset();
                    }
                }
            });

            this.HelpCommand = this.CreateCommand((parameter) =>
            {
                ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency,-Rank,-&-Inventory");
                return Task.FromResult(0);
            });
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                await DialogHelper.ShowMessage("An inventory name must be specified");
                return false;
            }

            UserInventoryModel dupeInventory = ChannelSession.Settings.Inventories.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeInventory != null && (this.inventory == null || !this.inventory.ID.Equals(dupeInventory.ID)))
            {
                await DialogHelper.ShowMessage("There already exists an inventory with this name");
                return false;
            }

            UserCurrencyModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.Name));
            if (dupeCurrency != null)
            {
                await DialogHelper.ShowMessage("There already exists a currency or rank system with this name");
                return false;
            }

            if (this.DefaultItemMaxAmount <= 0)
            {
                await DialogHelper.ShowMessage("The default max amount must be greater than 0");
                return false;
            }

            if (this.Items.Count() == 0)
            {
                await DialogHelper.ShowMessage("At least 1 item must be added");
                return false;
            }

            if (this.ShopEnabled)
            {
                if (string.IsNullOrEmpty(this.ShopCommandText))
                {
                    await DialogHelper.ShowMessage("A command name must be specified for the shop");
                    return false;
                }

                if (this.SelectedShopCurrency == null)
                {
                    await DialogHelper.ShowMessage("A currency must be specified for the shop");
                    return false;
                }
            }

            if (this.TradeEnabled)
            {
                if (string.IsNullOrEmpty(this.TradeCommandText))
                {
                    await DialogHelper.ShowMessage("A command name must be specified for trading");
                    return false;
                }
            }

            return true;
        }

        public void Save()
        {
            if (this.inventory == null)
            {
                this.inventory = new UserInventoryModel();
                ChannelSession.Settings.Inventories[this.inventory.ID] = this.inventory;
            }

            this.inventory.Name = this.Name;
            this.inventory.DefaultMaxAmount = this.DefaultItemMaxAmount;
            this.inventory.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.inventory.Name);
            this.inventory.Items = new Dictionary<string, UserInventoryItemModel>(this.Items.ToDictionary(i => i.Name, i => i));

            this.inventory.ShopEnabled = this.ShopEnabled;
            this.inventory.ShopCommand = this.ShopCommandText;
            if (this.SelectedShopCurrency != null)
            {
                this.inventory.ShopCurrencyID = this.SelectedShopCurrency.ID;
            }
            this.inventory.ItemsBoughtCommand = this.ShopBuyCommand;
            this.inventory.ItemsSoldCommand = this.ShopSellCommand;

            this.inventory.TradeEnabled = this.TradeEnabled;
            this.inventory.TradeCommand = this.TradeCommandText;
            this.inventory.ItemsTradedCommand = this.TradeCommand;
        }
    }
}
