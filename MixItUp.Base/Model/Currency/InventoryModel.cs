using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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
                return MixItUp.Base.Resources.Default;
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

        [JsonIgnore]
        public string MaxAmountSpecialIdentifier { get { return string.Format("{0}maxamount", this.SpecialIdentifier); } }

        [JsonIgnore]
        public string ShopBuyPriceSpecialIdentifier { get { return string.Format("{0}buyprice", this.SpecialIdentifier); } }
        [JsonIgnore]
        public string ShopSellPriceSpecialIdentifier { get { return string.Format("{0}sellprice", this.SpecialIdentifier); } }

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
        public UserV2ViewModel User { get; set; }
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
        public Guid ItemsBoughtCommandID { get; set; }
        [DataMember]
        public Guid ItemsSoldCommandID { get; set; }

        [DataMember]
        public bool TradeEnabled { get; set; }
        [DataMember]
        public string TradeCommand { get; set; }
        [DataMember]
        public Guid ItemsTradedCommandID { get; set; }

        [JsonIgnore]
        private DateTimeOffset shopListCooldown = DateTimeOffset.MinValue;

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
        public string UniqueItemsTotalSpecialIdentifier { get { return string.Format("{0}uniqueitemstotal", this.SpecialIdentifier); } }

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

        [JsonIgnore]
        public string UserUniqueItemsTotalSpecialIdentifier { get { return string.Format("{0}uniqueitemstotal", this.UserAmountSpecialIdentifierHeader); } }

        [JsonIgnore]
        public CommandModelBase ItemsBoughtCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.ItemsBoughtCommandID); }
            set
            {
                if (value != null)
                {
                    this.ItemsBoughtCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.ItemsBoughtCommandID);
                    this.ItemsBoughtCommandID = Guid.Empty;
                }
            }
        }

        [JsonIgnore]
        public CommandModelBase ItemsSoldCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.ItemsSoldCommandID); }
            set
            {
                if (value != null)
                {
                    this.ItemsSoldCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.ItemsSoldCommandID);
                    this.ItemsSoldCommandID = Guid.Empty;
                }
            }
        }

        [JsonIgnore]
        public CommandModelBase ItemsTradedCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.ItemsTradedCommandID); }
            set
            {
                if (value != null)
                {
                    this.ItemsTradedCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.ItemsTradedCommandID);
                    this.ItemsTradedCommandID = Guid.Empty;
                }
            }
        }

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

        public int GetAmount(UserV2ViewModel user, InventoryItemModel item)
        {
            if (user.InventoryAmounts.ContainsKey(this.ID) && user.InventoryAmounts[this.ID].ContainsKey(item.ID))
            {
                return user.InventoryAmounts[this.ID][item.ID];
            }
            return 0;
        }

        public int GetAmount(UserV2ViewModel user, Guid itemID)
        {
            if (this.ItemExists(itemID))
            {
                return this.GetAmount(user, this.GetItem(itemID));
            }
            return 0;
        }

        public int GetAmount(UserV2Model user, InventoryItemModel item)
        {
            if (user.InventoryAmounts.ContainsKey(this.ID) && user.InventoryAmounts[this.ID].ContainsKey(item.ID))
            {
                return user.InventoryAmounts[this.ID][item.ID];
            }
            return 0;
        }

        public int GetAmount(UserV2Model user, Guid itemID)
        {
            if (this.ItemExists(itemID))
            {
                return this.GetAmount(user, this.GetItem(itemID));
            }
            return 0;
        }

        public Dictionary<Guid, int> GetAmounts(UserV2Model user)
        {
            Dictionary<Guid, int> amounts = new Dictionary<Guid, int>();
            foreach (InventoryItemModel item in this.Items.Values)
            {
                amounts[item.ID] = this.GetAmount(user, item);
            }
            return amounts;
        }

        public Dictionary<Guid, int> GetAmounts(UserV2ViewModel user)
        {
            Dictionary<Guid, int> amounts = new Dictionary<Guid, int>();
            foreach (InventoryItemModel item in this.Items.Values)
            {
                amounts[item.ID] = this.GetAmount(user, item);
            }
            return amounts;
        }

        public bool HasAmount(UserV2ViewModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                return this.HasAmount(user, this.GetItem(itemID), amount);
            }
            return false;
        }

        public bool HasAmount(UserV2ViewModel user, InventoryItemModel item, int amount)
        {
            return (user.IsSpecialtyExcluded || this.GetAmount(user, item) >= amount);
        }

        public void SetAmount(UserV2ViewModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SetAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SetAmount(UserV2Model user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SetAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SetAmount(UserV2ViewModel user, InventoryItemModel item, int amount) { this.SetAmount(user.Model, item, amount); }

        public void SetAmount(UserV2Model user, InventoryItemModel item, int amount)
        {
            Logger.Log(LogLevel.Debug, $"Setting {amount} amount of item {item.Name} of {this.Name} for {user.ID}");

            if (!user.InventoryAmounts.ContainsKey(this.ID))
            {
                user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
            }
            user.InventoryAmounts[this.ID][item.ID] = Math.Min(Math.Max(amount, 0), item.HasMaxAmount ? item.MaxAmount : this.DefaultMaxAmount);

            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.Users.ManualValueChanged(user.ID);
            }
        }

        public void AddAmount(UserV2ViewModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.AddAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void AddAmount(UserV2Model user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.AddAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void AddAmount(UserV2ViewModel user, InventoryItemModel item, int amount)
        {
            if (!user.IsSpecialtyExcluded && amount > 0)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) + amount);
            }
        }

        public void AddAmount(UserV2Model user, InventoryItemModel item, int amount)
        {
            if (!user.IsSpecialtyExcluded && amount > 0)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) + amount);
            }
        }

        public void SubtractAmount(UserV2Model user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SubtractAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SubtractAmount(UserV2Model user, InventoryItemModel item, int amount)
        {
            if (!user.IsSpecialtyExcluded)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) - amount);
            }
        }

        public void SubtractAmount(UserV2ViewModel user, Guid itemID, int amount)
        {
            if (this.ItemExists(itemID))
            {
                this.SubtractAmount(user, this.GetItem(itemID), amount);
            }
        }

        public void SubtractAmount(UserV2ViewModel user, InventoryItemModel item, int amount)
        {
            if (!user.IsSpecialtyExcluded)
            {
                this.SetAmount(user, item, this.GetAmount(user, item) - amount);
            }
        }

        public void ResetAmount(UserV2ViewModel user)
        {
            user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
            ChannelSession.Settings.Users.ManualValueChanged(user.ID);
        }

        public async Task Reset()
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

            foreach (UserV2Model user in ChannelSession.Settings.Users.Values.ToList())
            {
                if (user.InventoryAmounts.ContainsKey(this.ID))
                {
                    user.InventoryAmounts[this.ID] = new Dictionary<Guid, int>();
                    ChannelSession.Settings.Users.ManualValueChanged(user.ID);
                }
            }
            await ChannelSession.SaveSettings();
        }

        public async Task PerformShopCommand(UserV2ViewModel user, IEnumerable<string> arguments = null)
        {
            try
            {
                if (ChannelSession.Settings.Currency.ContainsKey(this.ShopCurrencyID))
                {
                    CurrencyModel currency = ChannelSession.Settings.Currency[this.ShopCurrencyID];

                    if (arguments != null && arguments.Count() > 0)
                    {
                        string arg1 = arguments.ElementAt(0);
                        if (arguments.Count() == 1 && arg1.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Result cooldown = CooldownRequirementModel.GetCooldownAmountMessage(this.shopListCooldown);
                            if (!cooldown.Success)
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(cooldown.Message, user.Platform);
                                return;
                            }
                            this.shopListCooldown = DateTimeOffset.Now.AddSeconds(10);

                            List<string> items = new List<string>();
                            foreach (InventoryItemModel item in this.Items.Values)
                            {
                                if (item.HasBuyAmount || item.HasSellAmount)
                                {
                                    items.Add(item.Name);
                                }
                            }
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemBuySellAvailableHeader + string.Join(", ", items), user.Platform);
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
                                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ValidAmountGreaterThan0MustBeSpecified, user.Platform);
                                        return;
                                    }
                                }
                            }

                            if (item == null)
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemSpecifiedDoesNotExist, user.Platform);
                                return;
                            }

                            int totalcost = 0;
                            CommandModelBase command = null;
                            if (arg1.Equals("buy", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasBuyAmount)
                                {
                                    int itemMaxAmount = (item.HasMaxAmount) ? item.MaxAmount : this.DefaultMaxAmount;
                                    if ((this.GetAmount(user, item) + amount) <= itemMaxAmount)
                                    {
                                        totalcost = item.BuyAmount * amount;
                                        if (currency.HasAmount(user, totalcost))
                                        {
                                            currency.SubtractAmount(user, totalcost);
                                            this.AddAmount(user, item, amount);
                                            command = this.ItemsBoughtCommand;
                                        }
                                        else
                                        {
                                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryDoNotHaveRequiredCurrencyToPurchase, totalcost, currency.Name), user.Platform);
                                        }
                                    }
                                    else
                                    {
                                        await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryYouCanOnlyHaveTotal, itemMaxAmount, item.Name), user.Platform);
                                    }
                                }
                                else
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemNotAvailableForBuying, user.Platform);
                                }
                            }
                            else if (arg1.Equals("sell", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.HasSellAmount)
                                {
                                    totalcost = item.SellAmount * amount;
                                    if (this.HasAmount(user, item, amount))
                                    {
                                        this.SubtractAmount(user, item, amount);
                                        currency.AddAmount(user, totalcost);
                                        command = this.ItemsSoldCommand;
                                    }
                                    else
                                    {
                                        await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryDoNotHaveRequiredItemsToSell, amount, item.Name), user.Platform);
                                    }
                                }
                                else
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemNotAvailableForSelling, user.Platform);
                                }
                            }
                            else
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryMustSpecifyEitherBuyOrSell, "buy", "sell"), user.Platform);
                            }

                            if (command != null)
                            {
                                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                specialIdentifiers["itemtotal"] = amount.ToString();
                                specialIdentifiers["itemname"] = item.Name;
                                specialIdentifiers["itemcost"] = totalcost.ToString();
                                specialIdentifiers["currencyname"] = currency.Name;
                                await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(user, arguments: arguments, specialIdentifiers: specialIdentifiers));
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
                                        itemInfo.Append(string.Format(MixItUp.Base.Resources.InventoryItemBuyPrice, item.BuyAmount, currency.Name));
                                    }
                                    if (item.HasBuyAmount && item.HasSellAmount)
                                    {
                                        itemInfo.Append(string.Format(", "));
                                    }
                                    if (item.HasSellAmount)
                                    {
                                        itemInfo.Append(string.Format(MixItUp.Base.Resources.InventoryItemSellPrice, item.SellAmount, currency.Name));
                                    }

                                    await ServiceManager.Get<ChatService>().SendMessage(itemInfo.ToString(), user.Platform);
                                }
                                else
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemNotAvailableForBuyingSelling, user.Platform);
                                }
                            }
                            else
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemSpecifiedDoesNotExist, user.Platform);
                            }
                            return;
                        }
                    }

                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryShopUsage, this.ShopCommand, "list", "buy", "sell"), user.Platform);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task PerformTradeCommand(UserV2ViewModel user, IEnumerable<string> arguments = null)
        {
            try
            {
                if (arguments != null)
                {
                    if (this.tradeReceiver == null && arguments.Count() >= 2)
                    {
                        CommandParametersModel parameters = new CommandParametersModel(user.Platform, arguments);
                        parameters.ParseArguments();

                        UserV2ViewModel targetUser = ServiceManager.Get<UserService>().GetActiveUserByPlatform(user.Platform, platformUsername: parameters.Arguments.First());
                        if (targetUser == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.UserNotFound, user.Platform);
                            return;
                        }

                        int amount = 1;
                        IEnumerable<string> itemArgs = parameters.Arguments.Skip(1);
                        InventoryItemModel item = this.GetItem(string.Join(" ", itemArgs));

                        if (item == null && itemArgs.Count() > 1)
                        {
                            itemArgs = itemArgs.Take(itemArgs.Count() - 1);
                            item = this.GetItem(string.Join(" ", itemArgs));
                            if (item != null)
                            {
                                if (!int.TryParse(parameters.Arguments.Last(), out amount) || amount <= 0)
                                {
                                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ValidAmountGreaterThan0MustBeSpecified, user.Platform);
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemSpecifiedDoesNotExist, user.Platform);
                            return;
                        }

                        if (!this.HasAmount(user, item, amount))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryDoNotHaveRequiredItemsToTrade, amount, item.Name), user.Platform);
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
                        AsyncRunner.RunAsyncBackground(async (token) =>
                        {
                            await Task.Delay(60000);
                            if (!token.IsCancellationRequested)
                            {
                                this.tradeSender = null;
                                this.tradeReceiver = null;
                                this.tradeTimeCheckToken = null;
                                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryTradeCancelled, user.Platform);
                            }
                        }, this.tradeTimeCheckToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryTradeStarted, this.tradeSender.User.Username, this.tradeReceiver.User.Username, this.tradeSender.Amount, this.tradeSender.Item.Name, this.TradeCommand), this.tradeSender.User.Platform);
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
                                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ValidAmountGreaterThan0MustBeSpecified, user.Platform);
                                    return;
                                }
                            }
                        }

                        if (item == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryItemSpecifiedDoesNotExist, user.Platform);
                            return;
                        }

                        if (!this.HasAmount(user, item, amount))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryDoNotHaveRequiredItemsToTrade, amount, item.Name), user.Platform);
                            return;
                        }

                        this.tradeReceiver.Item = item;
                        this.tradeReceiver.Amount = amount;

                        await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryTradeReplied, this.tradeReceiver.User.Username, this.tradeSender.User.Username, this.tradeReceiver.Amount, this.tradeReceiver.Item.Name, this.TradeCommand), user.Platform);
                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && this.tradeReceiver.Amount > 0 && this.tradeSender.User.Equals(user))
                    {
                        int senderItemMaxAmount = (this.tradeReceiver.Item.HasMaxAmount) ? this.tradeReceiver.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeSender.User, this.tradeReceiver.Item) + this.tradeReceiver.Amount) > senderItemMaxAmount)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryYouCanOnlyHaveTotal, senderItemMaxAmount, this.tradeReceiver.Item.Name), this.tradeReceiver.User.Platform);
                            return;
                        }

                        int receiverItemMaxAmount = (this.tradeSender.Item.HasMaxAmount) ? this.tradeSender.Item.MaxAmount : this.DefaultMaxAmount;
                        if ((this.GetAmount(this.tradeReceiver.User, this.tradeSender.Item) + this.tradeSender.Amount) > receiverItemMaxAmount)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryYouCanOnlyHaveTotal, receiverItemMaxAmount, this.tradeSender.Item.Name), this.tradeSender.User.Platform);
                            return;
                        }

                        this.SubtractAmount(this.tradeSender.User, this.tradeSender.Item, this.tradeSender.Amount);
                        this.AddAmount(this.tradeReceiver.User, this.tradeSender.Item, this.tradeSender.Amount);

                        this.SubtractAmount(this.tradeReceiver.User, this.tradeReceiver.Item, this.tradeReceiver.Amount);
                        this.AddAmount(this.tradeSender.User, this.tradeReceiver.Item, this.tradeReceiver.Amount);

                        if (this.ItemsTradedCommand != null)
                        {
                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                            specialIdentifiers["itemtotal"] = this.tradeSender.Amount.ToString();
                            specialIdentifiers["itemname"] = this.tradeSender.Item.Name;
                            specialIdentifiers["targetitemtotal"] = this.tradeReceiver.Amount.ToString();
                            specialIdentifiers["targetitemname"] = this.tradeReceiver.Item.Name;
                            await ServiceManager.Get<CommandService>().Queue(this.ItemsTradedCommand, new CommandParametersModel(user, arguments: new string[] { this.tradeReceiver.User.Username }, specialIdentifiers: specialIdentifiers));
                        }

                        this.tradeSender = null;
                        this.tradeReceiver = null;
                        this.tradeTimeCheckToken.Cancel();
                        this.tradeTimeCheckToken = null;

                        return;
                    }
                    else if (this.tradeSender != null && this.tradeReceiver != null && !this.tradeReceiver.User.Equals(user))
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.InventoryTradeAlreadyUnderway, user.Platform);
                        return;
                    }
                }
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.InventoryTradeUsage, this.TradeCommand), user.Platform);
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
