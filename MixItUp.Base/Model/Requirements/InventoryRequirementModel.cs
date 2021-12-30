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
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public Guid InventoryID { get; set; }
        [DataMember]
        public Guid ItemID { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public InventoryRequirementModel(InventoryModel inventory, InventoryItemModel item, int amount)
        {
            this.InventoryID = inventory.ID;
            this.ItemID = item.ID;
            this.Amount = amount;
        }

        [Obsolete]
        public InventoryRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return InventoryRequirementModel.requirementErrorCooldown; } set { InventoryRequirementModel.requirementErrorCooldown = value; } }

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
            if (inventory != null && !parameters.User.IsSpecialtyExcluded)
            {
                inventory.SubtractAmount(parameters.User, this.ItemID, this.Amount);
            }
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            InventoryModel inventory = this.Inventory;
            if (inventory != null && !parameters.User.IsSpecialtyExcluded)
            {
                inventory.AddAmount(parameters.User, this.ItemID, this.Amount);
            }
            return Task.CompletedTask;
        }

        public Result ValidateAmount(UserV2ViewModel user, int amount)
        {
            if (!user.IsSpecialtyExcluded && !this.Inventory.HasAmount(user, this.ItemID, amount))
            {
                int currentAmount = this.Inventory.GetAmount(user, this.ItemID);
                return new Result(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amount, this.Item.Name) + " " + string.Format(MixItUp.Base.Resources.RequirementCurrentAmount, currentAmount));
            }
            return new Result();
        }
    }
}
