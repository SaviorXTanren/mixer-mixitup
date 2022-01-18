using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
{
    [DataContract]
    public class InventoryAmount
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<InventoryItemAmount> Items { get; set; } = new List<InventoryItemAmount>();
    }

    [DataContract]
    public class InventoryItemAmount
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }
    }
}
