using System;

namespace MixItUp.API.V2.Models
{
    public class InventoryRequirement : CommandRequirement
    {
        public Guid InventoryID { get; set; }
        public Guid ItemID { get; set; }
        public int Amount { get; set; }
    }
}
