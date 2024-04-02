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
        public Guid CommandID { get; set; }

        public RedemptionStoreProductModel()
        {
            this.ID = Guid.NewGuid();
        }

        [JsonIgnore]
        public bool IsInfinite { get { return this.MaxAmount < 0; } }

        [JsonIgnore]
        public CommandModelBase Command
        {
            get { return ChannelSession.Settings.GetCommand(this.CommandID); }
            set
            {
                if (value != null)
                {
                    this.CommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.CommandID);
                    this.CommandID = Guid.Empty;
                }
            }
        }

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
        public static event EventHandler OnRedemptionStorePurchasesUpdated = delegate { };
        public static void RedemptionStorePurchasesUpdated() { OnRedemptionStorePurchasesUpdated(null, new EventArgs()); }

        public static async Task Purchase(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            if (arguments.Count() == 0)
            {
                List<string> items = new List<string>();
                foreach (RedemptionStoreProductModel product in ChannelSession.Settings.RedemptionStoreProducts.Values.ToList())
                {
                    if (product.IsInfinite || product.CurrentAmount > 0)
                    {
                        items.Add(product.Name);
                    }
                }
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.RedemptionStoreProductsAvailableForPurchaseHeader + string.Join(", ", items), platform: user.Platform);
            }
            else
            {
                string name = string.Join(" ", arguments);
                RedemptionStoreProductModel product = ChannelSession.Settings.RedemptionStoreProducts.Values.ToList().FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (product == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoRedemptionStoreProductWithThatName, user.Platform);
                    return;
                }

                if (!product.IsInfinite)
                {
                    if (product.CurrentAmount <= 0)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoMoreRedemptionStoreProducts, user.Platform);
                        return;
                    }

                    ThresholdRequirementModel threshold = product.Requirements.Threshold;
                    if (threshold != null && threshold.IsEnabled && threshold.Amount > product.CurrentAmount)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NotEnoughRedemptionStoreProducts, user.Platform);
                        return;
                    }
                }

                Result result = await product.Requirements.Validate(new CommandParametersModel(user, arguments));
                if (result.Success)
                {
                    await product.Requirements.Perform(new CommandParametersModel(user, arguments));
                    foreach (CommandParametersModel u in product.Requirements.GetPerformingUsers(new CommandParametersModel(user, arguments)))
                    {
                        if (!product.IsInfinite)
                        {
                            product.CurrentAmount--;
                        }

                        RedemptionStorePurchaseModel purchase = new RedemptionStorePurchaseModel(product, u.User);
                        ChannelSession.Settings.RedemptionStorePurchases.Add(purchase);

                        if (product.AutoRedeem)
                        {
                            await purchase.Redeem();
                        }
                        else
                        {
                            purchase.State = RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded;
                            u.SpecialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = product.Name;

                            CommandModelBase command = ChannelSession.Settings.GetCommand(ChannelSession.Settings.RedemptionStoreManualRedeemNeededCommandID);
                            if (command != null)
                            {
                                await ServiceManager.Get<CommandService>().Queue(command, u);
                            }

                            RedemptionStorePurchaseModel.RedemptionStorePurchasesUpdated();
                        }
                    }
                }
            }
        }

        public static async Task Redeem(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            if (!user.MeetsRole(UserRoleEnum.Moderator))
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.YouDoNotHavePermissions, user.Platform);
                return;
            }

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
                name = UserService.SanitizeUsername(name);
                UserV2ViewModel purchaseUser = ServiceManager.Get<UserService>().GetActiveUserByPlatform(user.Platform, platformUsername: name);
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
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoRedemptionStorePurchasesWithThatName, user.Platform);
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

        [JsonIgnore]
        public UserV2ViewModel User { get; set; }

        public RedemptionStorePurchaseModel() { }

        public RedemptionStorePurchaseModel(RedemptionStoreProductModel product, UserV2ViewModel user)
        {
            this.ID = Guid.NewGuid();
            this.ProductID = product.ID;
            this.UserID = user.ID;
            this.PurchaseDate = DateTimeOffset.Now;

            this.User = user;
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

        public async Task Redeem()
        {
            RedemptionStoreProductModel product = this.Product;
            UserV2ViewModel user = this.User;
            if (product != null && user != null)
            {
                CommandModelBase command = product.Command;
                if (command == null)
                {
                    command = ChannelSession.Settings.GetCommand(ChannelSession.Settings.RedemptionStoreDefaultRedemptionCommandID);
                }

                if (command != null)
                {
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = product.Name;

                    await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(user, specialIdentifiers: specialIdentifiers));
                }

                if (this.State == RedemptionStorePurchaseRedemptionState.ManualRedeemNeeded)
                {
                    this.State = RedemptionStorePurchaseRedemptionState.ManualRedeemPerformed;
                }
                else
                {
                    this.State = RedemptionStorePurchaseRedemptionState.AutoRedeemed;
                }
            }
            RedemptionStorePurchaseModel.RedemptionStorePurchasesUpdated();
        }

        public async Task Refund()
        {
            RedemptionStoreProductModel product = this.Product;
            UserV2ViewModel user = this.User;
            if (product != null && user != null)
            {
                await product.Requirements.Refund(new CommandParametersModel(user));
                if (!product.IsInfinite)
                {
                    product.CurrentAmount++;
                }
            }
            this.Remove();
        }

        public void Remove()
        {
            ChannelSession.Settings.RedemptionStorePurchases.Remove(this);
            RedemptionStorePurchaseModel.RedemptionStorePurchasesUpdated();
        }
    }
}
