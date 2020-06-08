using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Currency
{
    [DataContract]
    public class RedemptionStoreProductModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public bool AutoRedeem { get; set; }

        [DataMember]
        public CustomCommand Command { get; set; }
    }
}
