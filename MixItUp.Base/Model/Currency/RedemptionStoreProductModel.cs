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
        public static async Task Purchase(UserViewModel user, IEnumerable<string> arguments)
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
                await ServiceManager.Get<ChatService>().SendMessage("Products Available to Purchase: " + string.Join(", ", items), platform: user.Platform);
            }
            else
            {
                string name = string.Join(" ", arguments);
                RedemptionStoreProductModel product = ChannelSession.Settings.RedemptionStoreProducts.Values.ToList().FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (product == null)
                {
                    if (ServiceManager.Get<ChatService>() != null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoRedemptionStoreProductWithThatName);
                    }
                    return;
                }

                if (!product.IsInfinite)
                {
                    if (product.CurrentAmount <= 0)
                    {
                        if (ServiceManager.Get<ChatService>() != null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoMoreRedemptionStoreProducts);
                        }
                        return;
                    }

                    ThresholdRequirementModel threshold = product.Requirements.Threshold;
                    if (threshold != null && threshold.IsEnabled && threshold.Amount > product.CurrentAmount)
                    {
                        if (ServiceManager.Get<ChatService>() != null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NotEnoughRedemptionStoreProducts);
                        }
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
                                await command.Perform(u);
                            }

                            GlobalEvents.RedemptionStorePurchasesUpdated();
                        }
                    }
                }
            }
        }

        public static async Task Redeem(UserViewModel user, IEnumerable<string> arguments)
        {
            if (!user.HasPermissionsTo(UserRoleEnum.Mod))
            {
                if (ServiceManager.Get<ChatService>() != null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.YouDoNotHavePermissions);
                }
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
                name = name.Replace("@", "");
                UserViewModel purchaseUser = ServiceManager.Get<UserService>().GetUserByUsername(name, user.Platform);
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
                if (ServiceManager.Get<ChatService>() != null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.NoRedemptionStorePurchasesWithThatName);
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
                UserViewModel user = ServiceManager.Get<UserService>().GetUserByID(this.UserID);
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
                CommandModelBase command = product.Command;
                if (command == null)
                {
                    command = ChannelSession.Settings.GetCommand(ChannelSession.Settings.RedemptionStoreDefaultRedemptionCommandID);
                }

                if (command != null)
                {
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                    specialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = product.Name;

                    await command.Perform(new CommandParametersModel(user, specialIdentifiers: specialIdentifiers));
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
            GlobalEvents.RedemptionStorePurchasesUpdated();
        }

        public async Task Refund()
        {
            RedemptionStoreProductModel product = this.Product;
            UserViewModel user = this.User;
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
            GlobalEvents.RedemptionStorePurchasesUpdated();
        }
    }
}
