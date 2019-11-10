using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Currency;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class CurrencyRankInventoryContainer
    {
        public UserCurrencyViewModel Currency { get; private set; }
        public UserInventoryViewModel Inventory { get; private set; }

        public CurrencyRankInventoryContainer(UserCurrencyViewModel currency) { this.Currency = currency; }

        public CurrencyRankInventoryContainer(UserInventoryViewModel inventory) { this.Inventory = inventory; }

        public string Name
        {
            get
            {
                if (this.Inventory != null) { return this.Inventory.Name; }
                else { return this.Currency.Name; }
            }
        }

        public string Type
        {
            get
            {
                if (this.Inventory != null) { return "Inventory"; }
                else if (this.Currency.IsRank) { return "Rank"; }
                else { return "Currency"; }
            }
        }

        public string AmountSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Inventory != null)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAllAmountSpecialIdentifier);
                }
                else
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.Top10SpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }

        public string RankSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Currency != null && this.Currency.IsRank)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserRankNameSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserRankNameSpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }
    }

    /// <summary>
    /// Interaction logic for CurrencyRankInventoryControl.xaml
    /// </summary>
    public partial class CurrencyRankInventoryControl : MainControlBase
    {
        private ObservableCollection<CurrencyRankInventoryContainer> items = new ObservableCollection<CurrencyRankInventoryContainer>();

        public CurrencyRankInventoryControl()
        {
            InitializeComponent();

            this.MainDataGrid.ItemsSource = this.items;

            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatCommandMessageReceived;
        }

        public void RefreshList()
        {
            this.items.Clear();
            foreach (var kvp in ChannelSession.Settings.Currencies.ToDictionary())
            {
                if (kvp.Value.IsRank)
                {
                    this.items.Add(new CurrencyRankInventoryContainer(kvp.Value));
                }
                else
                {
                    this.items.Add(new CurrencyRankInventoryContainer(kvp.Value));
                }
            }
            foreach (var kvp in ChannelSession.Settings.Inventories.ToDictionary())
            {
                this.items.Add(new CurrencyRankInventoryContainer(kvp.Value));
            }
        }

        public async void DeleteItem(CurrencyRankInventoryContainer item)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you wish to delete this?"))
                {
                    if (item.Inventory != null)
                    {
                        await item.Inventory.Reset();
                        ChannelSession.Settings.Inventories.Remove(item.Inventory.ID);
                    }
                    else
                    {
                        await item.Currency.Reset();
                        ChannelSession.Settings.Currencies.Remove(item.Currency.ID);
                    }
                    this.RefreshList();
                }
            });
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshList();
            await base.InitializeInternal();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrencyRankInventoryContainer item = (CurrencyRankInventoryContainer)button.DataContext;
            if (item.Inventory != null)
            {
                InventoryWindow window = new InventoryWindow(item.Inventory);
                window.Closed += Window_Closed;
                window.Show();
            }
            else
            {
                CurrencyWindow window = new CurrencyWindow(item.Currency);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrencyRankInventoryContainer item = (CurrencyRankInventoryContainer)button.DataContext;
            this.DeleteItem(item);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }

        private void AddNewCurrencyRankButton_Click(object sender, RoutedEventArgs e)
        {
            CurrencyWindow window = new CurrencyWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void AddNewInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            InventoryWindow window = new InventoryWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void GlobalEvents_OnChatCommandMessageReceived(object sender, ChatMessageViewModel message)
        {
            foreach (UserInventoryViewModel inventory in ChannelSession.Settings.Inventories.Values)
            {
                if (inventory.ShopEnabled && message.PlainTextMessage.StartsWith(inventory.ShopCommand))
                {
                    string args = message.PlainTextMessage.Replace(inventory.ShopCommand, "");
                    await inventory.PerformShopCommand(message.User, args.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                }
                else if (inventory.TradeEnabled && message.PlainTextMessage.StartsWith(inventory.TradeCommand))
                {
                    string args = message.PlainTextMessage.Replace(inventory.TradeCommand, "");
                    await inventory.PerformTradeCommand(message.User, args.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }
    }
}
