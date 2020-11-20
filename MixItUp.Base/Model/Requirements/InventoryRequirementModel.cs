using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class InventoryRequirementModel : RequirementModelBase
    {
        [DataMember]
        public Guid InventoryID { get; set; }
        [DataMember]
        public Guid ItemID { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public InventoryRequirementModel() { }

        public InventoryRequirementModel(InventoryModel inventory, InventoryItemModel item, int amount)
        {
            this.InventoryID = inventory.ID;
            this.ItemID = item.ID;
            this.Amount = amount;
        }

        internal InventoryRequirementModel(MixItUp.Base.ViewModel.Requirement.InventoryRequirementViewModel requirement)
        {
            this.InventoryID = requirement.InventoryID;
            this.ItemID = requirement.ItemID;
            this.Amount = requirement.Amount;
        }

        [JsonIgnore]
        public InventoryModel Inventory
        {
            get
            {
                if (ChannelSession.Settings.Inventory.ContainsKey(this.InventoryID))
                {
                    return ChannelSession.Settings.Inventory[this.InventoryID];
                }
                return null;
            }
        }

        [JsonIgnore]
        public InventoryItemModel Item
        {
            get
            {
                InventoryModel inventory = this.Inventory;
                if (inventory != null && inventory.Items.ContainsKey(this.ItemID))
                {
                    return inventory.Items[this.ItemID];
                }
                return null;
            }
        }

        public override async Task<bool> Validate(CommandParametersModel parameters)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory == null)
            {
                return false;
            }

            InventoryItemModel item = this.Item;
            if (item == null)
            {
                return false;
            }

            if (!parameters.User.Data.IsCurrencyRankExempt && !inventory.HasAmount(parameters.User.Data, item.ID, this.Amount))
            {
                await this.SendChatMessage(string.Format("You do not have the required {0} {1} to do this", this.Amount, item.Name));
                return false;
            }

            return true;
        }

        public override Task Perform(CommandParametersModel parameters)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                inventory.SubtractAmount(parameters.User.Data, this.ItemID, this.Amount);
            }
            return Task.FromResult(0);
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                inventory.AddAmount(parameters.User.Data, this.ItemID, this.Amount);
            }
            return Task.FromResult(0);
        }
    }
}
