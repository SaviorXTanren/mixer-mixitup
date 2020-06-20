using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.User;
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

        public override async Task<bool> Validate(UserViewModel user)
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

            if (!user.Data.IsCurrencyRankExempt && !inventory.HasAmount(user.Data, item, this.Amount))
            {
                await this.SendChatWhisper(user, string.Format("You do not have the required {0} {1} to do this", this.Amount, item.Name));
                return false;
            }

            return true;
        }

        public override Task Perform(UserViewModel user)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !user.Data.IsCurrencyRankExempt)
            {
                inventory.SubtractAmount(user.Data, this.ItemID, this.Amount);
            }
            return Task.FromResult(0);
        }

        public override Task Refund(UserViewModel user)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !user.Data.IsCurrencyRankExempt)
            {
                inventory.AddAmount(user.Data, this.ItemID, this.Amount);
            }
            return Task.FromResult(0);
        }
    }
}
