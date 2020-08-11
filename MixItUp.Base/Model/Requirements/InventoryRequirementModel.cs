using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public string Amount { get; set; }

        public InventoryRequirementModel() { }

        public InventoryRequirementModel(InventoryModel inventory, InventoryItemModel item, string amount)
        {
            this.InventoryID = inventory.ID;
            this.ItemID = item.ID;
            this.Amount = amount;
        }

        internal InventoryRequirementModel(MixItUp.Base.ViewModel.Requirement.InventoryRequirementViewModel requirement)
        {
            this.InventoryID = requirement.InventoryID;
            this.ItemID = requirement.ItemID;
            this.Amount = requirement.Amount.ToString();
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

        public override async Task<bool> Validate(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
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

            if (!user.Data.IsCurrencyRankExempt && !inventory.HasAmount(user.Data, item.ID, await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers)))
            {
                await this.SendChatMessage(string.Format("You do not have the required {0} {1} to do this", this.Amount, item.Name));
                return false;
            }

            return true;
        }

        public override async Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !user.Data.IsCurrencyRankExempt)
            {
                inventory.SubtractAmount(user.Data, this.ItemID, await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers));
            }
        }

        public override async Task Refund(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !user.Data.IsCurrencyRankExempt)
            {
                inventory.AddAmount(user.Data, this.ItemID, await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers));
            }
        }
    }
}
