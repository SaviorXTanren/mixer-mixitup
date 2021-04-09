using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
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

#pragma warning disable CS0612 // Type or member is obsolete
        internal InventoryRequirementModel(MixItUp.Base.ViewModel.Requirement.InventoryRequirementViewModel requirement)
        {
            this.InventoryID = requirement.InventoryID;
            this.ItemID = requirement.ItemID;
            this.Amount = requirement.Amount;
        }
#pragma warning restore CS0612 // Type or member is obsolete

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

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.InventoryDoesNotExist));
            }

            InventoryItemModel item = this.Item;
            if (item == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.InventoryItemDoesNotExist));
            }

            return Task.FromResult(this.ValidateAmount(parameters.User, this.Amount));
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            await base.Perform(parameters);
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                inventory.SubtractAmount(parameters.User.Data, this.ItemID, this.Amount);
            }
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

        public Result ValidateAmount(UserViewModel user, int amount)
        {
            if (!user.Data.IsCurrencyRankExempt && !this.Inventory.HasAmount(user.Data, this.ItemID, amount))
            {
                int currentAmount = this.Inventory.GetAmount(user.Data, this.ItemID);
                return new Result(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amount, this.Item.Name) + " " + string.Format(MixItUp.Base.Resources.RequirementCurrentAmount, currentAmount));
            }
            return new Result();
        }
    }
}
