using System;

namespace MixItUp.API.V2.Models
{
    public class ConsumablesAction : ActionBase
    {
        public Guid CurrencyID { get; set; }
        public Guid InventoryID { get; set; }
        public Guid StreamPassID { get; set; }
        public string ActionType { get; set; }
        public string Username { get; set; }
        public string ItemName { get; set; }
        public string Amount { get; set; }
        public bool DeductFromUser { get; set; }
        public string UserRoleToApplyTo { get; set; }
        public bool UsersMustBePresent { get; set; }
    }
}
