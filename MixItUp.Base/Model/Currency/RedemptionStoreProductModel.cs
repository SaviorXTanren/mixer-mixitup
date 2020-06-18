using MixItUp.Base.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Currency
{
    public enum RedemptionStorePurchaseRedemptionState
    {
        AutoRedeemed = 0,

        ManualRedeemNeeded = 1,
        ManualRedeemPerformed = 2,
    }

    [DataContract]
    public class RedemptionStoreProductModel
    {
        public const string ProductNameSpecialIdentifier = "productname";

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }
        [DataMember]
        public int CurrentAmount { get; set; }

        [DataMember]
        public bool AutoReplenish { get; set; }

        [DataMember]
        public bool AutoRedeem { get; set; }

        [DataMember]
        public RequirementsSetModel Requirements { get; set; }

        [DataMember]
        public CustomCommand Command { get; set; }

        public RedemptionStoreProductModel()
        {
            this.ID = Guid.NewGuid();
        }

        [JsonIgnore]
        public bool IsInfinite { get { return this.MaxAmount < 0; } }

        public void ReplenishAmount()
        {
            if (!this.IsInfinite && this.AutoReplenish)
            {
                this.CurrentAmount = this.MaxAmount;
            }
        }
    }

    [DataContract]
    public class RedemptionStorePurchaseModel
    {
        public const string ManualRedemptionNeededCommandName = "Redemption Store Manual Redeem Needed";
        public const string DefaultRedemptionCommandName = "Redemption Store Default Redemption";

        public static async Task Purchase(UserViewModel user, IEnumerable<string> arguments)
        {
            string name = string.Join(" ", arguments);
            RedemptionStoreProductModel product = ChannelSession.Settings.RedemptionStoreProducts.Values.ToList().FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (product == null)
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.Whisper(user, MixItUp.Base.Resources.NoRedemptionStoreProductWithThatName);
                }
                return;
            }

            if (!product.IsInfinite)
            {
                if (product.CurrentAmount <= 0)
                {
                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user, MixItUp.Base.Resources.NoMoreRedemptionStoreProducts);
                    }
                    return;
                }

                ThresholdRequirementModel threshold = product.Requirements.Threshold;
                if (threshold != null && threshold.IsEnabled && threshold.Amount > product.CurrentAmount)
                {
                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user, MixItUp.Base.Resources.NotEnoughRedemptionStoreProducts);
                    }
                    return;
                }
            }

            if (await product.Requirements.Validate(user))
            {
                await product.Requirements.Perform(user);
                foreach (UserViewModel u in product.Requirements.GetPerformingUsers(user))
                {
                    if (!product.IsInfinite)
                    {
                        product.CurrentAmount--;
                    }

                    RedemptionStorePurchaseModel purchase = new RedemptionStorePurchaseModel(product, u);
                    ChannelSession.Settings.RedemptionStorePurchases.Add(purchase);

                    if (product.AutoRedeem)
                    {
                        await purchase.Redeem();
                    }
                    else
                    {
                        purchase.State = RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded;

                        Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                        extraSpecialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = product.Name;

                        await ChannelSession.Settings.RedemptionStoreManualRedeemNeededCommand.Perform(u, extraSpecialIdentifiers: extraSpecialIdentifiers);
                    }
                }
            }
        }

        public static async Task Redeem(UserViewModel user, IEnumerable<string> arguments)
        {
            string name = string.Join(" ", arguments);
            RedemptionStorePurchaseModel purchase = null;

            RedemptionStoreProductModel product = ChannelSession.Settings.RedemptionStoreProducts.Values.ToList().FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (product != null)
            {
                IEnumerable<RedemptionStorePurchaseModel> purchases = ChannelSession.Settings.RedemptionStorePurchases.ToList().Where(p => p.ProductID == product.ID);
                if (purchases != null && purchases.Count() > 0)
                {
                    purchase = purchases.OrderBy(p => p.PurchaseDate).FirstOrDefault();
                }
            }
            else
            {
                name = name.Replace("@", "");
                UserViewModel purchaseUser = ChannelSession.Services.User.GetUserByUsername(name);
                if (purchaseUser != null)
                {
                    IEnumerable<RedemptionStorePurchaseModel> purchases = ChannelSession.Settings.RedemptionStorePurchases.ToList().Where(p => p.UserID == user.ID);
                    if (purchases != null && purchases.Count() > 0)
                    {
                        purchase = purchases.OrderBy(p => p.PurchaseDate).FirstOrDefault();
                    }
                }
            }

            if (purchase != null)
            {
                await purchase.Redeem();
            }
            else
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.Whisper(user, MixItUp.Base.Resources.NoRedemptionStorePurchasesWithThatName);
                }
            }
        }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Guid UserID { get; set; }

        [DataMember]
        public Guid ProductID { get; set; }

        [DataMember]
        public DateTimeOffset PurchaseDate { get; set; }

        [DataMember]
        public RedemptionStorePurchaseRedemptionState State { get; set; } = RedemptionStorePurchaseRedemptionState.AutoRedeemed;

        public RedemptionStorePurchaseModel() { }

        public RedemptionStorePurchaseModel(RedemptionStoreProductModel product, UserViewModel user)
        {
            this.ID = Guid.NewGuid();
            this.ProductID = product.ID;
            this.UserID = user.ID;
            this.PurchaseDate = DateTimeOffset.Now;
        }

        [JsonIgnore]
        public RedemptionStoreProductModel Product
        {
            get
            {
                if (ChannelSession.Settings.RedemptionStoreProducts.ContainsKey(this.ProductID))
                {
                    return ChannelSession.Settings.RedemptionStoreProducts[this.ProductID];
                }
                return null;
            }
        }

        [JsonIgnore]
        public UserViewModel User
        {
            get
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByID(this.UserID);
                if (user == null)
                {
                    UserDataModel userData = ChannelSession.Settings.GetUserData(this.UserID);
                    if (user != null)
                    {
                        user = new UserViewModel(userData);
                    }
                }
                return user;
            }
        }

        public async Task Redeem()
        {
            RedemptionStoreProductModel product = this.Product;
            UserViewModel user = this.User;
            if (product != null && user != null)
            {
                CustomCommand command = product.Command;
                if (command == null)
                {
                    command = ChannelSession.Settings.RedemptionStoreDefaultRedemptionCommand;
                }

                Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                extraSpecialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = product.Name;

                await command.Perform(user, extraSpecialIdentifiers: extraSpecialIdentifiers);

                if (this.State == RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded)
                {
                    this.State = RedemptionStorePurchaseRedemptionState.ManualRedeemPerformed;
                }
                else
                {
                    this.State = RedemptionStorePurchaseRedemptionState.AutoRedeemed;
                }
            }
        }

        public async Task Refund()
        {
            RedemptionStoreProductModel product = this.Product;
            UserViewModel user = this.User;
            if (product != null && user != null)
            {
                await product.Requirements.Refund(user);
                if (!product.IsInfinite)
                {
                    product.CurrentAmount++;
                }
            }
        }
    }
}
