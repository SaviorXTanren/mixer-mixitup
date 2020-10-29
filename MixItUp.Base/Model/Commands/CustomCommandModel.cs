using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Window.Currency;
using System.Collections.Generic;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    public class CustomCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public CustomCommandModel(string name) : base(name, CommandTypeEnum.Custom) { }

        internal CustomCommandModel(MixItUp.Base.Commands.CustomCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.Custom;
        }

        protected override SemaphoreSlim CommandLockSemaphore { get { return CustomCommandModel.commandLockSemaphore; } }

        public override Dictionary<string, string> GetUniqueSpecialIdentifiers()
        {
            Dictionary<string, string> specialIdentifiers = base.GetUniqueSpecialIdentifiers();
            if (this.Name.Equals(MixItUp.Base.Resources.InventoryItemsBoughtCommandName) || this.Name.Equals(MixItUp.Base.Resources.InventoryItemsSoldCommandName))
            {
                specialIdentifiers["itemtotal"] = "5";
                specialIdentifiers["itemname"] = "Chocolate Bars";
                specialIdentifiers["itemcost"] = "500";
                specialIdentifiers["currencyname"] = "CURRENCY_NAME";
            }
            else if (this.Name.Contains(MixItUp.Base.Resources.ModerationStrikeCommandName))
            {
                specialIdentifiers[ModerationService.ModerationReasonSpecialIdentifier] = "Bad Stuff";
            }
            else if (this.Name.Equals(MixItUp.Base.Resources.RedemptionStoreManualRedeemNeededCommandName) || this.Name.Equals(MixItUp.Base.Resources.RedemptionStoreDefaultRedemptionCommandName))
            {
                specialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = "Test Product";
            }
            else if (this.Name.Equals(MixItUp.Base.Resources.GameQueueUserJoinedCommandName) || this.Name.Equals(MixItUp.Base.Resources.GameQueueUserSelectedCommandName))
            {
                specialIdentifiers["queueposition"] = "1";
            }
            return specialIdentifiers;
        }
    }
}
