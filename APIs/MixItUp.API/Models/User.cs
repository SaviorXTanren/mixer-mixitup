using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int? ViewingMinutes { get; set; }

        [DataMember]
        public List<CurrencyAmount> CurrencyAmounts { get; set; } = new List<CurrencyAmount>();

        [DataMember]
        public List<InventoryAmount> InventoryAmounts { get; set; } = new List<InventoryAmount>();

        [DataMember]
        public List<string> ParticipantIDs { get; set; } = new List<string>();
    }
}