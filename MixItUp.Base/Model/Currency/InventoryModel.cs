using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Currency
{
    [DataContract]
    public class InventoryItemModel : IEquatable<InventoryItemModel>
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [DataMember]
        public int BuyAmount { get; set; }

        [DataMember]
        public int SellAmount { get; set; }

        public InventoryItemModel()
        {
            this.ID = Guid.NewGuid();
        }

        public InventoryItemModel(string name, int maxAmount = -1, int buyAmount = -1, int sellAmount = -1)
            : this()
        {
            this.Name = name;
            this.MaxAmount = maxAmount;
            this.BuyAmount = buyAmount;
            this.SellAmount = sellAmount;
        }

        [JsonIgnore]
        public bool HasMaxAmount { get { return this.MaxAmount > 0; } }

        [JsonIgnore]
        public string MaxAmountString
        {
            get
            {
                if (this.HasMaxAmount)
                {
                    return this.MaxAmount.ToString();
                }
                return "Default";
            }
        }

        [JsonIgnore]
        public bool HasBuyAmount { get { return this.BuyAmount > 0; } }

        [JsonIgnore]
        public string BuyAmountString
        {
            get
            {
                if (this.HasBuyAmount)
                {
                    return this.BuyAmount.ToString();
                }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public bool HasSellAmount { get { return this.SellAmount > 0; } }

        [JsonIgnore]
        public string SellAmountString
        {
            get
            {
                if (this.HasSellAmount)
                {
                    return this.SellAmount.ToString();
                }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public string SpecialIdentifier { get { return SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name); } }

        public override bool Equals(object obj)
        {
            if (obj is InventoryItemModel)
            {
                return this.Equals((InventoryItemModel)obj);
            }
            return false;
        }

        public bool Equals(InventoryItemModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }

    public class InventoryTradeModel
    {
        public UserViewModel User { get; set; }
        public InventoryItemModel Item { get; set; }
        public int Amount { get; set; }
    }

    [DataContract]
    public class InventoryModel : IEquatable<InventoryModel>
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int DefaultMaxAmount { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public Dictionary<Guid, InventoryItemModel> Items { get; set; } = new Dictionary<Guid, InventoryItemModel>();

        [DataMember]
        public bool ShopEnabled { get; set; }
        [DataMember]
        public string ShopCommand { get; set; }
        [DataMember]
        public Guid ShopCurrencyID { get; set; }
        [DataMember]
        public CustomCommand ItemsBoughtCommand { get; set; }
        [DataMember]
        public CustomCommand ItemsSoldCommand { get; set; }

        [DataMember]
        public bool TradeEnabled { get; set; }
        [DataMember]
        public string TradeCommand { get; set; }
        [DataMember]
        public CustomCommand ItemsTradedCommand { get; set; }

        [JsonIgnore]
        private InventoryTradeModel tradeSender = null;
        [JsonIgnore]
        private InventoryTradeModel tradeReceiver = null;
        [JsonIgnore]
        private CancellationTokenSource tradeTimeCheckToken = null;

        public InventoryModel()
        {
            this.ID = Guid.NewGuid();
            this.DefaultMaxAmount = 99;
            this.SpecialIdentifier = string.Empty;
        }

        [JsonIgnore]
        public string RandomItemSpecialIdentifier { get { return string.Format("{0}randomitem", this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierHeader { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierExample { get { return string.Format("{0}<ITEM>", this.UserAmountSpecialIdentifierHeader); } }

        [JsonIgnore]
        public string UserAllAmountSpecialIdentifier { get { return string.Format("{0}all", this.UserAmountSpecialIdentifierHeader); } }

        [JsonIgnore]
        public string UserRandomItemSpecialIdentifier { get { return string.Format("{0}randomitem", this.UserAmountSpecialIdentifierHeader); } }

        public bool ItemExists(Guid itemID) { return this.Items.ContainsKey(itemID); }

        public bool ItemExists(string itemName) { return this.GetItem(itemName) != null; }

        public InventoryItemModel GetItem(string itemName) { return this.Items.Values.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase)); }

        public InventoryItemModel GetItem(Guid itemID)
        {
            if (this.ItemExists(itemID))
            {
                return this.Items[itemID];
            }
            return null;
        }

        public int GetAmount(UserDataModel user, InventoryItemModel item)
        {
            if (user.InventoryAmounts.ContainsKey(this.ID) && user.InventoryAmounts[this.ID].ContainsKey(item.ID))
            {
                return user.InventoryAmounts[this.ID][item.ID];
            }
            return 0;
        }

        public int GetAmount(UserDataModel user, Guid itemID)
        {
            if (this.ItemExists(itemID))
            {
                return this.GetAmount(user, this.GetItem(itemID));
            }
            return 0;
        }

        public Dictionary<Guid, int> GetAmounts(UserDataModel user)
        {
            Dictionary<Guid, int> amounts = new Dictionary<Guid, int>();
            foreach (InventoryItemModel item in this.Items.Values)
            {
                amounts[item.ID] = this.GetAmount(user, item);
            }
            return amounts;
        }

        public bool HasAmount(UserDataModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                return this.HasAmount(user, this.GetItem(itemID), amount);
            }
            return false;
        }

        public bool HasAmount(UserDataModel user, InventoryItemModel item, int amount)
        {
            return (user.IsCurrencyRankExempt || this.GetAmount(user, item) >= amount);
        }

        public void SetAmount(UserDataModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SetAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SetAmount(UserDataModel user, InventoryItemModel item, int amount)
        {
            if (!user.InventoryAmounts.ContainsKey(this.ID))
            {
                user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
            }
            user.InventoryAmounts[this.ID][item.ID] = Math.Min(Math.Max(amount, 0), item.HasMaxAmount ? item.MaxAmount : this.DefaultMaxAmount);

            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
            }
        }

        public void AddAmount(UserDataModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.AddAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void AddAmount(UserDataModel user, InventoryItemModel item, int amount)
        {
            if (!user.IsCurrencyRankExempt && amount > 0)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) + amount);
            }
        }

        public void SubtractAmount(UserDataModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SubtractAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SubtractAmount(UserDataModel user, InventoryItemModel item, int amount)
        {
            if (!user.IsCurrencyRankExempt)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) - amount);
            }
        }

        public void ResetAmount(UserDataModel user)
        {
            user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
            ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
        }

        public async Task Reset()
        {
            foreach (UserDataModel user in ChannelSession.Settings.UserData.Values.ToList())
            {
                if (user.InventoryAmounts.ContainsKey(this.ID))
                {
                    user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
                    ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
                }
            }
            await ChannelSession.SaveSettings();
        }

        public async Task PerformShopCommand(UserViewModel user, IEnumerable<string> arguments = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None)
        {
            try
            {
                if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Currency.ContainsKey(this.ShopCurrencyID))
                {
                    CurrencyModel currency = ChannelSession.Settings.Currency[this.ShopCurrencyID];

                    if (arguments != null && arguments.Count() > 0)
                    {
                        string arg1 = arguments.ElementAt(0);
                        if (arguments.Count() == 1 && arg1.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                        {
                            List<string> items = new List<string>();
                            foreach (InventoryItemModel item in this.Items.Values)
                            {
                                if (item.HasBuyAmount || item.HasSellAmount)
                                {
                                    items.Add(item.Name);
                                }
                            }
                            await ChannelSession.Services.Chat.Whisper(user, "Items Available to Buy/Sell: " + string.Join(", ", items));
                            return;
                        }
                        else if (arguments.Count() >= 2 &&
                            (arg1.Equals("buy", StringComparison.InvariantCultureIgnoreCase) || arg1.Equals("sell", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            int amount = 1;

                            IEnumerable<string> itemArgs = arguments.Skip(1);
                            InventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));
                            if (item == null && itemArgs.Count() > 1)
                            {
                                itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                                item = this.GetItem(string.Join(" ", itemArgs));
                                if (item != null)
                                {
                                    if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user, "A valid amount greater than 0 must be specified");
                                        return;
                                    }
                                }
                            }

                            if (item == null)
                            {
                                await ChannelSession.Services.Chat.Whisper(user, "The item you specified does not exist");
                                return;
                            }

                            int totalcost = 0;
                            CustomCommand command = null;
                            if (arg1.Equals("buy", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasBuyAmount)
                                {
                                    int itemMaxAmount = (item.HasMaxAmount) ? item.MaxAmount : this.DefaultMaxAmount;
                                    if ((this.GetAmount(user.Data, item) + amount) <= itemMaxAmount)
                                    {
                                        totalcost = item.BuyAmount * amount;
                                        if (currency.HasAmount(user.Data, totalcost))
                                        {
                                            currency.SubtractAmount(user.Data, totalcost);
                                            this.AddAmount(user.Data, item, amount);
                                            command = this.ItemsBoughtCommand;
                                        }
                                        else
                                        {
                                            await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to purchase this item", totalcost, currency.Name));
                                        }
                                    }
                                    else
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user, string.Format("You can only have {0} {1} in total", itemMaxAmount, item.Name));
                                    }
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user, "This item is not available for buying");
                                }
                            }
                            else if (arg1.Equals("sell", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasSellAmount)
                                {
                                    totalcost = item.SellAmount * amount;
                                    if (this.HasAmount(user.Data, item, amount))
                                    {
                                        this.SubtractAmount(user.Data, item, amount);
                                        currency.AddAmount(user.Data, totalcost);
                                        command = this.ItemsSoldCommand;
                                    }
                                    else
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to sell", amount, item.Name));
                                    }
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user, "This item is not available for selling");
                                }
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user, "You must specify either \"buy\" & \"sell\"");
                            }

                            if (command != null)
                            {
                                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                specialIdentifiers["itemtotal"] = amount.ToString();
                                specialIdentifiers["itemname"] = item.Name;
                                specialIdentifiers["itemcost"] = totalcost.ToString();
                                specialIdentifiers["currencyname"] = currency.Name;
                                await command.Perform(user, arguments: arguments, extraSpecialIdentifiers: specialIdentifiers);
                            }
                            return;
                        }
                        else
                        {
                            InventoryItemModel item = this.GetItem(string.Join(" ", arguments));
                            if (item != null)
                            {
                                if (item.HasBuyAmount || item.HasSellAmount)
                                {
                                    StringBuilder itemInfo = new StringBuilder();
                                    itemInfo.Append(item.Name + ": ");
                                    if (item.HasBuyAmount)
                                    {
                                        itemInfo.Append(string.Format("Buy = {0} {1}", item.BuyAmount, currency.Name));
                                    }
                                    if (item.HasBuyAmount && item.HasSellAmount)
                                    {
                                        itemInfo.Append(string.Format(", "));
                                    }
                                    if (item.HasSellAmount)
                                    {
                                        itemInfo.Append(string.Format("Sell = {0} {1}", item.SellAmount, currency.Name));
                                    }

                                    await ChannelSession.Services.Chat.Whisper(user, itemInfo.ToString());
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user, "This item is not available to buy/sell");
                                }
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user, "The item you specified does not exist");
                            }
                            return;
                        }
                    }

                    StringBuilder storeHelp = new StringBuilder();
                    storeHelp.Append(this.ShopCommand + " list = Lists all the items available for buying/selling ** ");
                    storeHelp.Append(this.ShopCommand + " <ITEM NAME> = Lists the buying/selling price for the item ** ");
                    storeHelp.Append(this.ShopCommand + " buy <ITEM NAME> [AMOUNT] = Buys 1 or the amount specified of the item ** ");
                    storeHelp.Append(this.ShopCommand + " sell <ITEM NAME> [AMOUNT] = Sells 1 or the amount specified of the item");
                    await ChannelSession.Services.Chat.Whisper(user, storeHelp.ToString());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task PerformTradeCommand(UserViewModel user, IEnumerable<string> arguments = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None)
        {
            try
            {
                if (ChannelSession.Services.Chat != null && arguments != null)
                {
                    if (this.tradeReceiver == null && arguments.Count() >= 2)
                    {
                        UserViewModel targetUser = await SpecialIdentifierStringBuilder.GetUserFromArgument(arguments.First(), platform);
                        if (targetUser == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user, "The specified user does not exist");
                            return;
                        }

                        int amount = 1;
                        IEnumerable<string> itemArgs = arguments.Skip(1);
                        InventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));

                        if (item == null && itemArgs.Count() > 1)
                        {
                            itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                            item = this.GetItem(string.Join(" ", itemArgs));
                            if (item != null)
                            {
                                if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user, "A valid amount greater than 0 must be specified");
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user, "The item you specified does not exist");
                            return;
                        }

                        if (!this.HasAmount(user.Data, item, amount))
                        {
                            await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to trade", amount, item.Name));
                            return;
                        }

                        this.tradeSender = new InventoryTradeModel()
                        {
                            User = user,
                            Item = item,
                            Amount = amount
                        };

                        this.tradeReceiver = new InventoryTradeModel()
                        {
                            User = targetUser
                        };

                        this.tradeTimeCheckToken = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunBackgroundTask(this.tradeTimeCheckToken.Token, async (token) =>
                        {
                            await Task.Delay(60000);
                            if (!token.IsCancellationRequested)
                            {
                                this.tradeSender = null;
                                this.tradeReceiver = null;
                                this.tradeTimeCheckToken = null;
                                await ChannelSession.Services.Chat.SendMessage("The trade could not be completed in time and was cancelled...");
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        await ChannelSession.Services.Chat.SendMessage(string.Format("@{0} has started a trade with @{1} for {2} {3}. Type {4} <ITEM NAME> [AMOUNT] in chat to reply back with your offer in the next 60 seconds.", this.tradeSender.User.Username, this.tradeReceiver.User.Username, this.tradeSender.Amount, this.tradeSender.Item.Name, this.TradeCommand));
                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && this.tradeReceiver.User.Equals(user) && this.tradeReceiver.Amount == 0 && arguments.Count() >= 1)
                    {
                        int amount = 1;
                        IEnumerable<string> itemArgs = arguments.ToList();
                        InventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));

                        if (item == null && itemArgs.Count() > 1)
                        {
                            itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                            item = this.GetItem(string.Join(" ", itemArgs));
                            if (item != null)
                            {
                                if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user, "A valid amount greater than 0 must be specified");
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user, "The item you specified does not exist");
                            return;
                        }

                        if (!this.HasAmount(user.Data, item, amount))
                        {
                            await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to trade", amount, item.Name));
                            return;
                        }

                        this.tradeReceiver.Item = item;
                        this.tradeReceiver.Amount = amount;

                        await ChannelSession.Services.Chat.SendMessage(string.Format("@{0} has replied back to the offer by @{1} with {2} {3}. Type {4} in chat to accept the trade.", this.tradeReceiver.User.Username, this.tradeSender.User.Username, this.tradeReceiver.Amount, this.tradeReceiver.Item.Name, this.TradeCommand));
                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && this.tradeReceiver.Amount > 0 && this.tradeSender.User.Equals(user))
                    {
                        int senderItemMaxAmount = (this.tradeReceiver.Item.HasMaxAmount) ? this.tradeReceiver.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeSender.User.Data, this.tradeReceiver.Item) + this.tradeReceiver.Amount) > senderItemMaxAmount)
                        {
                            await ChannelSession.Services.Chat.Whisper(this.tradeSender.User, string.Format("You can only have {0} {1} in total", senderItemMaxAmount, this.tradeReceiver.Item.Name));
                            return;
                        }

                        int receiverItemMaxAmount = (this.tradeSender.Item.HasMaxAmount) ? this.tradeSender.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeReceiver.User.Data, this.tradeSender.Item) + this.tradeSender.Amount) > receiverItemMaxAmount)
                        {
                            await ChannelSession.Services.Chat.Whisper(this.tradeReceiver.User, string.Format("You can only have {0} {1} in total", receiverItemMaxAmount, this.tradeSender.Item.Name));
                            return;
                        }

                        this.SubtractAmount(this.tradeSender.User.Data, this.tradeSender.Item, this.tradeSender.Amount);
                        this.AddAmount(this.tradeReceiver.User.Data, this.tradeSender.Item, this.tradeSender.Amount);

                        this.SubtractAmount(this.tradeReceiver.User.Data, this.tradeReceiver.Item, this.tradeReceiver.Amount);
                        this.AddAmount(this.tradeSender.User.Data, this.tradeReceiver.Item, this.tradeReceiver.Amount);

                        if (this.ItemsTradedCommand != null)
                        {
                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                            specialIdentifiers["itemtotal"] = this.tradeSender.Amount.ToString();
                            specialIdentifiers["itemname"] = this.tradeSender.Item.Name;
                            specialIdentifiers["targetitemtotal"] = this.tradeReceiver.Amount.ToString();
                            specialIdentifiers["targetitemname"] = this.tradeReceiver.Item.Name;
                            await this.ItemsTradedCommand.Perform(user, arguments: new string[] { this.tradeReceiver.User.Username }, extraSpecialIdentifiers: specialIdentifiers);
                        }

                        this.tradeSender = null;
                        this.tradeReceiver = null;
                        this.tradeTimeCheckToken.Cancel();
                        this.tradeTimeCheckToken = null;

                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && !this.tradeReceiver.User.Equals(user))
                    {
                        await ChannelSession.Services.Chat.Whisper(user, "A trade is already underway, please wait until it is completed");
                        return;
                    }
                }
                await ChannelSession.Services.Chat.Whisper(user, this.TradeCommand + " <USERNAME> <ITEM NAME> [AMOUNT] = Trades 1 or the amount specified of the item to the specified user");
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryModel)
            {
                return this.Equals((InventoryModel)obj);
            }
            return false;
        }

        public bool Equals(InventoryModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
