using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserInventoryItemViewModel : IEquatable<UserInventoryItemViewModel>
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

        public UserInventoryItemViewModel(string name, int maxAmount = -1, int buyAmount = -1, int sellAmount = -1)
        {
            this.Name = name;
            this.MaxAmount = maxAmount;
            this.BuyAmount = buyAmount;
            this.SellAmount = sellAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryItemViewModel)
            {
                return this.Equals((UserInventoryItemViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryItemViewModel other)
        {
            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    [DataContract]
    public class UserInventoryViewModel : IEquatable<UserInventoryViewModel>
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
        public Dictionary<string, UserInventoryItemViewModel> Items { get; set; }

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

        public UserInventoryViewModel()
        {
            this.ID = Guid.NewGuid();
            this.DefaultMaxAmount = 99;
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = CurrencyResetRateEnum.Never;
            this.LastReset = DateTimeOffset.MinValue;

            this.Items = new Dictionary<string, UserInventoryItemViewModel>();
        }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierExample { get { return string.Format("{0}{1}<ITEM>", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierHeader { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAllAmountSpecialIdentifier { get { return string.Format("{0}{1}all", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        public async Task Reset()
        {
            foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values)
            {
                userData.ResetInventoryAmount(this);
                ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
            }
            await ChannelSession.SaveSettings();
        }

        public async Task PerformShopCommand(UserViewModel user, IEnumerable<string> arguments = null)
        {
            try
            {
                if (ChannelSession.Chat != null && ChannelSession.Settings.Currencies.ContainsKey(this.ShopCurrencyID))
                {
                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[this.ShopCurrencyID];

                    if (arguments != null)
                    {
                        if (arguments.Count() == 1)
                        {
                            string arg1 = arguments.ElementAt(0);
                            if (arg1.Equals("list"))
                            {
                                List<string> items = new List<string>();
                                foreach (UserInventoryItemViewModel item in this.Items.Values)
                                {
                                    if (item.HasBuyAmount || item.HasSellAmount)
                                    {
                                        items.Add(item.Name);
                                    }
                                }
                                await ChannelSession.Chat.Whisper(user.UserName, "Items Available to Buy/Sell: " + string.Join(", ", items));
                                return;
                            }
                            else
                            {
                                foreach (UserInventoryItemViewModel item in this.Items.Values)
                                {
                                    if (item.Name.Equals(arg1, StringComparison.InvariantCultureIgnoreCase))
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

                                            await ChannelSession.Chat.Whisper(user.UserName, itemInfo.ToString());
                                            return;
                                        }
                                        else
                                        {
                                            await ChannelSession.Chat.Whisper(user.UserName, "This item is not available to buy/sell");
                                            return;
                                        }
                                    }
                                }

                                await ChannelSession.Chat.Whisper(user.UserName, "The item you specified does not exist");
                                return;
                            }
                        }
                        else if (arguments.Count() == 2 || arguments.Count() == 3)
                        {
                            bool buying = false;
                            bool selling = false;
                            if (arguments.ElementAt(0).Equals("buy", StringComparison.InvariantCultureIgnoreCase))
                            {
                                buying = true;
                            }
                            else if (arguments.ElementAt(0).Equals("sell", StringComparison.InvariantCultureIgnoreCase))
                            {
                                selling = true;
                            }
                            else
                            {
                                await ChannelSession.Chat.Whisper(user.UserName, "You must specify either \"buy\" & \"sell\"");
                                return;
                            }

                            int amount = 1;
                            if (arguments.Count() == 3)
                            {
                                if (!int.TryParse(arguments.ElementAt(2), out amount) || amount <= 0)
                                {
                                    await ChannelSession.Chat.Whisper(user.UserName, "A valid amount greater than 0 must be specified");
                                    return;
                                }
                            }

                            string itemName = arguments.ElementAt(1);
                            CustomCommand command = null;
                            int totalcost = 0;
                            foreach (UserInventoryItemViewModel item in this.Items.Values)
                            {
                                if (item.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (buying)
                                    {
                                        if (item.HasBuyAmount)
                                        {
                                            totalcost = item.BuyAmount * amount;
                                            if (user.Data.HasCurrencyAmount(currency, totalcost))
                                            {
                                                user.Data.SubtractCurrencyAmount(currency, totalcost);
                                                user.Data.AddInventoryAmount(this, item.Name, amount);
                                                command = this.ItemsBoughtCommand;
                                            }
                                            else
                                            {
                                                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to purchase this item", totalcost, currency.Name));
                                            }
                                        }
                                        else
                                        {
                                            await ChannelSession.Chat.Whisper(user.UserName, "This item is not available for buying");
                                        }
                                    }
                                    else if (selling)
                                    {
                                        if (item.HasSellAmount)
                                        {
                                            totalcost = item.SellAmount * amount;
                                            if (user.Data.HasInventoryAmount(this, item.Name, amount))
                                            {
                                                user.Data.SubtractInventoryAmount(this, item.Name, amount);
                                                user.Data.AddCurrencyAmount(currency, totalcost);
                                                command = this.ItemsSoldCommand;
                                            }
                                            else
                                            {
                                                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to sell", amount, item.Name));
                                            }
                                        }
                                        else
                                        {
                                            await ChannelSession.Chat.Whisper(user.UserName, "This item is not available for selling");
                                        }
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
                            }

                            await ChannelSession.Chat.Whisper(user.UserName, "The item you specified does not exist");
                            return;
                        }
                    }

                    StringBuilder storeHelp = new StringBuilder();
                    storeHelp.Append(this.ShopCommand + " list = Lists all the items available for buying/selling ** ");
                    storeHelp.Append(this.ShopCommand + " <ITEM NAME> = Lists the buying/selling price for the item ** ");
                    storeHelp.Append(this.ShopCommand + " buy <ITEM NAME> [AMOUNT] = Buys 1 or the amount specified of the item ** ");
                    storeHelp.Append(this.ShopCommand + " sell <ITEM NAME> [AMOUNT] = Sells 1 or the amount specified of the item");
                    await ChannelSession.Chat.Whisper(user.UserName, storeHelp.ToString());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryViewModel)
            {
                return this.Equals((UserInventoryViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryViewModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
