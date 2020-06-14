using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class InventoryRequirementViewModel : IEquatable<InventoryRequirementViewModel>
    {
        [DataMember]
        public Guid InventoryID { get; set; }

        [DataMember]
        public Guid ItemID { get; set; }
        [DataMember]
        [Obsolete]
        public string ItemName { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public InventoryRequirementViewModel() { }

        public InventoryRequirementViewModel(InventoryModel inventory, InventoryItemModel item, int amount)
        {
            this.InventoryID = inventory.ID;
            this.ItemID = item.ID;
            this.Amount = amount;
        }

        public InventoryModel GetInventory()
        {
            if (ChannelSession.Settings.Inventory.ContainsKey(this.InventoryID))
            {
                return ChannelSession.Settings.Inventory[this.InventoryID];
            }
            return null;
        }

        public bool TrySubtractAmount(UserDataModel userData, bool requireAmount = false) { return this.TrySubtractAmount(userData, this.Amount, requireAmount); }

        public bool TrySubtractAmount(UserDataModel userData, int amount, bool requireAmount = false)
        {
            if (this.DoesMeetRequirement(userData))
            {
                InventoryModel inventory = this.GetInventory();
                if (inventory == null)
                {
                    return false;
                }

                if (requireAmount && !inventory.HasAmount(userData, this.ItemID, amount))
                {
                    return false;
                }

                inventory.SubtractAmount(userData, this.ItemID, amount);
                return true;
            }
            return false;
        }

        public bool DoesMeetRequirement(UserDataModel userData)
        {
            if (userData.IsCurrencyRankExempt)
            {
                return true;
            }

            InventoryModel inventory = this.GetInventory();
            if (inventory == null)
            {
                return false;
            }

#pragma warning disable CS0612 // Type or member is obsolete
            if (!string.IsNullOrEmpty(this.ItemName))
            {
                InventoryItemModel item = inventory.GetItem(this.ItemName);
                if (item != null)
                {
                    this.ItemID = item.ID;
                }
                this.ItemName = null;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            return inventory.HasAmount(userData, this.ItemID, this.Amount);
        }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Services.Chat != null && ChannelSession.Settings.Inventory.ContainsKey(this.InventoryID))
            {
                InventoryModel inventory = this.GetInventory();
                if (inventory != null && inventory.ItemExists(this.ItemID))
                {
                    await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} {1} to do this", this.Amount, inventory.GetItem(this.ItemID).Name));
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user, string.Format("You do not have the required {0} items to do this", this.Amount));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryRequirementViewModel)
            {
                return this.Equals((InventoryRequirementViewModel)obj);
            }
            return false;
        }

        public bool Equals(InventoryRequirementViewModel other) { return this.InventoryID.Equals(other.InventoryID); }

        public override int GetHashCode() { return this.InventoryID.GetHashCode(); }
    }
}
