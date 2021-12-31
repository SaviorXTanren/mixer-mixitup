using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class CustomCommandModel : CommandModelBase
    {
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

        [Obsolete]
        public CustomCommandModel() : base() { }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return CustomCommandModel.GetCustomTestSpecialIdentifiers(this.Name); }
    }
}
