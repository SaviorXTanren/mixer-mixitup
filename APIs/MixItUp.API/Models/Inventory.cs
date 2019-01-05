using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class Inventory
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }

    [DataContract]
    public class InventoryItem
    {
        [DataMember]
        public string Name { get; set; }
    }
}
