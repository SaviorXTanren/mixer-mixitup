using MixItUp.Base.Commands;
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

namespace MixItUp.Base.Model.User
{
    [DataContract]
    public class UserInventoryItemModel : IEquatable<UserInventoryItemModel>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [DataMember]
        public int BuyAmount { get; set; }

        [DataMember]
        public int SellAmount { get; set; }

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

        public UserInventoryItemModel(string name, int maxAmount = -1, int buyAmount = -1, int sellAmount = -1)
        {
            this.Name = name;
            this.MaxAmount = maxAmount;
            this.BuyAmount = buyAmount;
            this.SellAmount = sellAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryItemModel)
            {
                return this.Equals((UserInventoryItemModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryItemModel other)
        {
            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    public class UserInventoryTradeModel
    {
        public UserViewModel User { get; set; }
        public UserInventoryItemModel Item { get; set; }
        public int Amount { get; set; }
    }

    [DataContract]
    public class UserInventoryModel : IEquatable<UserInventoryModel>
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int DefaultMaxAmount { get; set; }

        [DataMember]
        public CurrencyResetRateEnum ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public Dictionary<string, UserInventoryItemModel> Items { get; set; }

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

        [JsonProperty]
        public DatabaseDictionary<Guid, Dictionary<string, int>> UserAmounts { get; set; } = new DatabaseDictionary<Guid, Dictionary<string, int>>();

        [JsonIgnore]
        private UserInventoryTradeModel tradeSender = null;
        [JsonIgnore]
        private UserInventoryTradeModel tradeReceiver = null;
        [JsonIgnore]
        private CancellationTokenSource tradeTimeCheckToken = null;

        public UserInventoryModel()
        {
            this.ID = Guid.NewGuid();
            this.DefaultMaxAmount = 99;
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = CurrencyResetRateEnum.Never;
            this.LastReset = DateTimeOffset.MinValue;

            this.Items = new Dictionary<string, UserInventoryItemModel>();
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

        public int GetAmount(UserDataViewModel user, UserInventoryItemModel item)
        {
            return this.GetAmount(user, item.Name);
        }

        public int GetAmount(UserDataViewModel user, string item)
        {
            if (this.UserAmounts.ContainsKey(user.ID) && this.UserAmounts[user.ID].ContainsKey(item))
            {
                return this.UserAmounts[user.ID][item];
            }
            return 0;
        }

        public Dictionary<string, int> GetAmounts(UserDataViewModel user) { return this.GetAmounts(user.ID); }

        public Dictionary<string, int> GetAmounts(Guid userID)
        {
            Dictionary<string, int> amounts = new Dictionary<string, int>();
            foreach (string itemName in this.Items.Keys)
            {
                amounts[itemName] = 0;
                if (this.UserAmounts.ContainsKey(userID) && this.UserAmounts[userID].ContainsKey(itemName))
                {
                    amounts[itemName] = this.UserAmounts[userID][itemName];
                }
            }
            return amounts;
        }

        public bool HasAmount(UserDataViewModel user, string item, int amount)
        {
            return (user.IsCurrencyRankExempt || this.GetAmount(user, item) >= amount);
        }

        public void SetAmount(UserDataViewModel user, string itemName, int amount)
        {
            if (this.Items.ContainsKey(itemName))
            {
                UserInventoryItemModel item = this.Items[itemName];
                if (!this.UserAmounts.ContainsKey(user.ID))
                {
                    this.UserAmounts[user.ID] = new Dictionary<string, int>();
                }
                this.UserAmounts[user.ID][itemName] = Math.Min(Math.Max(amount, 0), item.HasMaxAmount ? item.MaxAmount : this.DefaultMaxAmount);
                ChannelSession.Settings.UserData.ManualValueChanged(user.MixerID);
            }
        }

        public void AddAmount(UserDataViewModel user, string item, int amount)
        {
            if (!user.IsCurrencyRankExempt)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) + amount);
            }
        }

        public void SubtractAmount(UserDataViewModel user, string item, int amount)
        {
            if (!user.IsCurrencyRankExempt)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) - amount);
            }
        }

        public void ResetAmount(UserDataViewModel user)
        {
            this.UserAmounts[user.ID] = new Dictionary<string, int>();
            ChannelSession.Settings.UserData.ManualValueChanged(user.MixerID);
        }

        public async Task Reset()
        {
            foreach (Guid key in this.UserAmounts.Keys)
            {
                this.UserAmounts[key] = new Dictionary<string, int>();
                this.UserAmounts.ManualValueChanged(key);
            }
            await ChannelSession.SaveSettings();
        }

        public async Task PerformShopCommand(UserViewModel user, IEnumerable<string> arguments = null)
        {
            try
            {
                if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.ShopCurrencyID))
                {
                    UserCurrencyModel currency = ChannelSession.Settings.Currencies[this.ShopCurrencyID];

                    if (arguments != null && arguments.Count() > 0)
                    {
                        string arg1 = arguments.ElementAt(0);
                        if (arguments.Count() == 1 && arg1.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                        {
                            List<string> items = new List<string>();
                            foreach (UserInventoryItemModel item in this.Items.Values)
                            {
                                if (item.HasBuyAmount || item.HasSellAmount)
                                {
                                    items.Add(item.Name);
                                }
                            }
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "Items Available to Buy/Sell: " + string.Join(", ", items));
                            return;
                        }
                        else if (arguments.Count() >= 2 &&
                            (arg1.Equals("buy", StringComparison.InvariantCultureIgnoreCase) || arg1.Equals("sell", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            int amount = 1;

                            IEnumerable<string> itemArgs = arguments.Skip(1);
                            UserInventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));
                            if (item == null && itemArgs.Count() > 1)
                            {
                                itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                                item = this.GetItem(string.Join(" ", itemArgs));
                                if (item != null)
                                {
                                    if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "A valid amount greater than 0 must be specified");
                                        return;
                                    }
                                }
                            }

                            if (item == null)
                            {
                                await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "The item you specified does not exist");
                                return;
                            }

                            int totalcost = 0;
                            CustomCommand command = null;
                            if (arg1.Equals("buy", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasBuyAmount)
                                {
                                    int itemMaxAmount = (item.HasMaxAmount) ? item.MaxAmount : this.DefaultMaxAmount;
                                    if ((this.GetAmount(user.Data, item.Name) + amount) <= itemMaxAmount)
                                    {
                                        totalcost = item.BuyAmount * amount;
                                        if (user.Data.HasCurrencyAmount(currency, totalcost))
                                        {
                                            user.Data.SubtractCurrencyAmount(currency, totalcost);
                                            this.AddAmount(user.Data, item.Name, amount);
                                            command = this.ItemsBoughtCommand;
                                        }
                                        else
                                        {
                                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to purchase this item", totalcost, currency.Name));
                                        }
                                    }
                                    else
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You can only have {0} {1} in total", itemMaxAmount, item.Name));
                                    }
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "This item is not available for buying");
                                }
                            }
                            else if (arg1.Equals("sell", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasSellAmount)
                                {
                                    totalcost = item.SellAmount * amount;
                                    if (this.HasAmount(user.Data, item.Name, amount))
                                    {
                                        this.SubtractAmount(user.Data, item.Name, amount);
                                        user.Data.AddCurrencyAmount(currency, totalcost);
                                        command = this.ItemsSoldCommand;
                                    }
                                    else
                                    {
                                        await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to sell", amount, item.Name));
                                    }
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "This item is not available for selling");
                                }
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "You must specify either \"buy\" & \"sell\"");
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
                            UserInventoryItemModel item = this.GetItem(string.Join(" ", arguments));
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

                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, itemInfo.ToString());
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "This item is not available to buy/sell");
                                }
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "The item you specified does not exist");
                            }
                            return;
                        }
                    }

                    StringBuilder storeHelp = new StringBuilder();
                    storeHelp.Append(this.ShopCommand + " list = Lists all the items available for buying/selling ** ");
                    storeHelp.Append(this.ShopCommand + " <ITEM NAME> = Lists the buying/selling price for the item ** ");
                    storeHelp.Append(this.ShopCommand + " buy <ITEM NAME> [AMOUNT] = Buys 1 or the amount specified of the item ** ");
                    storeHelp.Append(this.ShopCommand + " sell <ITEM NAME> [AMOUNT] = Sells 1 or the amount specified of the item");
                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, storeHelp.ToString());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task PerformTradeCommand(UserViewModel user, IEnumerable<string> arguments = null)
        {
            try
            {
                if (ChannelSession.Services.Chat != null && arguments != null)
                {
                    if (this.tradeReceiver == null && arguments.Count() >= 2)
                    {
                        UserViewModel targetUser = await SpecialIdentifierStringBuilder.GetUserFromArgument(arguments.First());
                        if (targetUser == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "The specified user does not exist");
                            return;
                        }

                        int amount = 1;
                        IEnumerable<string> itemArgs = arguments.Skip(1);
                        UserInventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));

                        if (item == null && itemArgs.Count() > 1)
                        {
                            itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                            item = this.GetItem(string.Join(" ", itemArgs));
                            if (item != null)
                            {
                                if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "A valid amount greater than 0 must be specified");
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "The item you specified does not exist");
                            return;
                        }

                        if (!this.HasAmount(user.Data, item.Name, amount))
                        {
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to trade", amount, item.Name));
                            return;
                        }

                        this.tradeSender = new UserInventoryTradeModel()
                        {
                            User = user,
                            Item = item,
                            Amount = amount
                        };

                        this.tradeReceiver = new UserInventoryTradeModel()
                        {
                            User = targetUser
                        };

                        this.tradeTimeCheckToken = new CancellationTokenSource();
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

                        await ChannelSession.Services.Chat.SendMessage(string.Format("@{0} has started a trade with @{1} for {2} {3}. Type {4} <ITEM NAME> [AMOUNT] in chat to reply back with your offer in the next 60 seconds.", this.tradeSender.User.MixerUsername, this.tradeReceiver.User.MixerUsername, this.tradeSender.Amount, this.tradeSender.Item.Name, this.TradeCommand));
                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && this.tradeReceiver.User.Equals(user) && this.tradeReceiver.Amount == 0 && arguments.Count() >= 1)
                    {
                        int amount = 1;
                        IEnumerable<string> itemArgs = arguments.ToList();
                        UserInventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));

                        if (item == null && itemArgs.Count() > 1)
                        {
                            itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                            item = this.GetItem(string.Join(" ", itemArgs));
                            if (item != null)
                            {
                                if (!int.TryParse(arguments.Last(), out amount) || amount <= 0)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "A valid amount greater than 0 must be specified");
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "The item you specified does not exist");
                            return;
                        }

                        if (!this.HasAmount(user.Data, item.Name, amount))
                        {
                            await ChannelSession.Services.Chat.Whisper(user.MixerUsername, string.Format("You do not have the required {0} {1} to trade", amount, item.Name));
                            return;
                        }

                        this.tradeReceiver.Item = item;
                        this.tradeReceiver.Amount = amount;

                        await ChannelSession.Services.Chat.SendMessage(string.Format("@{0} has replied back to the offer by @{1} with {2} {3}. Type {4} in chat to accept the trade.", this.tradeReceiver.User.MixerUsername, this.tradeSender.User.MixerUsername, this.tradeReceiver.Amount, this.tradeReceiver.Item.Name, this.TradeCommand));
                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && this.tradeReceiver.Amount > 0 && this.tradeSender.User.Equals(user))
                    {
                        int senderItemMaxAmount = (this.tradeReceiver.Item.HasMaxAmount) ? this.tradeReceiver.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeSender.User.Data, this.tradeReceiver.Item.Name) + this.tradeReceiver.Amount) > senderItemMaxAmount)
                        {
                            await ChannelSession.Services.Chat.Whisper(this.tradeSender.User.MixerUsername, string.Format("You can only have {0} {1} in total", senderItemMaxAmount, this.tradeReceiver.Item.Name));
                            return;
                        }

                        int receiverItemMaxAmount = (this.tradeSender.Item.HasMaxAmount) ? this.tradeSender.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeReceiver.User.Data, this.tradeSender.Item.Name) + this.tradeSender.Amount) > receiverItemMaxAmount)
                        {
                            await ChannelSession.Services.Chat.Whisper(this.tradeReceiver.User.MixerUsername, string.Format("You can only have {0} {1} in total", receiverItemMaxAmount, this.tradeSender.Item.Name));
                            return;
                        }

                        this.SubtractAmount(this.tradeSender.User.Data, this.tradeSender.Item.Name, this.tradeSender.Amount);
                        this.AddAmount(this.tradeReceiver.User.Data, this.tradeSender.Item.Name, this.tradeSender.Amount);

                        this.SubtractAmount(this.tradeReceiver.User.Data, this.tradeReceiver.Item.Name, this.tradeReceiver.Amount);
                        this.AddAmount(this.tradeSender.User.Data, this.tradeReceiver.Item.Name, this.tradeReceiver.Amount);

                        if (this.ItemsTradedCommand != null)
                        {
                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                            specialIdentifiers["itemtotal"] = this.tradeSender.Amount.ToString();
                            specialIdentifiers["itemname"] = this.tradeSender.Item.Name;
                            specialIdentifiers["targetitemtotal"] = this.tradeReceiver.Amount.ToString();
                            specialIdentifiers["targetitemname"] = this.tradeReceiver.Item.Name;
                            await this.ItemsTradedCommand.Perform(user, arguments: new string[] { this.tradeReceiver.User.MixerUsername }, extraSpecialIdentifiers: specialIdentifiers);
                        }

                        this.tradeSender = null;
                        this.tradeReceiver = null;
                        this.tradeTimeCheckToken.Cancel();
                        this.tradeTimeCheckToken = null;

                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && !this.tradeReceiver.User.Equals(user))
                    {
                        await ChannelSession.Services.Chat.Whisper(user.MixerUsername, "A trade is already underway, please wait until it is completed");
                        return;
                    }
                }
                await ChannelSession.Services.Chat.Whisper(user.MixerUsername, this.TradeCommand + " <USERNAME> <ITEM NAME> [AMOUNT] = Trades 1 or the amount specified of the item to the specified user");
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryModel)
            {
                return this.Equals((UserInventoryModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        private UserInventoryItemModel GetItem(string itemName)
        {
            foreach (UserInventoryItemModel item in this.Items.Values)
            {
                if (item.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }
    }
}