using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Currency
{
    public enum RedemptionStorePurchaseRedemptionState
    {
        AutoRedeemed = 0,
        ManualRedeemNeeded = 1,
        ManualRedeemPerformed = 2
    }

    [DataContract]
    public class RedemptionStoreProductModel
    {
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
    }
}
