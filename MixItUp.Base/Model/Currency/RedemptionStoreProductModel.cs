using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public CustomCommand Command { get; set; }

        public RedemptionStoreProductModel()
        {
            this.ID = Guid.NewGuid();
        }

        [JsonIgnore]
        public int EditableQuantity { get { return (this.AutoReplenish) ? this.MaxAmount : this.CurrentAmount; } }
    }

    [DataContract]
    public class RedemptionStorePurchaseModel
    {
        public const string ManualRedemptionNeededCommandName = "Redemption Store Manual Redeem Needed";
        public const string DefaultRedemptionCommandName = "Redemption Store Default Redemption";

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Guid UserID { get; set; }

        [DataMember]
        public Guid ProductID { get; set; }

        [DataMember]
        public DateTimeOffset PurchaseDate { get; set; }

        [DataMember]
        public RedemptionStorePurchaseRedemptionState State { get; set; }

        public RedemptionStorePurchaseModel()
        {
            this.ID = Guid.NewGuid();
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

        }
    }
}
