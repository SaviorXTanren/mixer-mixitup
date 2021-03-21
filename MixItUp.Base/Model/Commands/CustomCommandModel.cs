using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class CustomCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public static Dictionary<string, string> GetCustomTestSpecialIdentifiers(string name)
        {
            Dictionary<string, string> specialIdentifiers = CommandModelBase.GetGeneralTestSpecialIdentifiers();
            if (name.Equals(MixItUp.Base.Resources.InventoryItemsBoughtCommandName) || name.Equals(MixItUp.Base.Resources.InventoryItemsSoldCommandName))
            {
                specialIdentifiers["itemtotal"] = "5";
                specialIdentifiers["itemname"] = "Chocolate Bars";
                specialIdentifiers["itemcost"] = "500";
                specialIdentifiers["currencyname"] = "CURRENCY_NAME";
            }
            else if (name.Contains(MixItUp.Base.Resources.ModerationStrikeCommandName))
            {
                specialIdentifiers[ModerationService.ModerationReasonSpecialIdentifier] = "Bad Stuff";
            }
            else if (name.Equals(MixItUp.Base.Resources.RedemptionStoreManualRedeemNeededCommandName) || name.Equals(MixItUp.Base.Resources.RedemptionStoreDefaultRedemptionCommandName))
            {
                specialIdentifiers[RedemptionStoreProductModel.ProductNameSpecialIdentifier] = "Test Product";
            }
            else if (name.Equals(MixItUp.Base.Resources.GameQueueUserJoinedCommandName) || name.Equals(MixItUp.Base.Resources.GameQueueUserSelectedCommandName))
            {
                specialIdentifiers["queueposition"] = "1";
            }
            return specialIdentifiers;
        }

        public CustomCommandModel(string name) : base(name, CommandTypeEnum.Custom) { }

#pragma warning disable CS0612 // Type or member is obsolete
        internal CustomCommandModel(MixItUp.Base.Commands.CustomCommand command)
            : base(command)
        {
            if (command != null)
            {
                this.Name = command.Name;
                this.Type = CommandTypeEnum.Custom;
            }
            else
            {
                this.Name = MixItUp.Base.Resources.CustomCommand;
                this.Type = CommandTypeEnum.Custom;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        protected CustomCommandModel() : base() { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return CustomCommandModel.commandLockSemaphore; } }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return CustomCommandModel.GetCustomTestSpecialIdentifiers(this.Name); }
    }
}
