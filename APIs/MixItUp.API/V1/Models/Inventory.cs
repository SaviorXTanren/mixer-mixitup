using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.V1.Models
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

        [DataMember]
        public Guid ShopCurrencyID { get; set; }
    }

    [DataContract]
    public class InventoryItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int BuyAmount { get; set; }

        [DataMember]
        public int SellAmount { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }
    }
}
