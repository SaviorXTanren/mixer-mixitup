using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class GiveawayAction : ActionBase
    {
        [DataMember]
        public string GiveawayItem { get; set; }

        public GiveawayAction() { }

        public GiveawayAction(string giveawayItem)
            : base(ActionTypeEnum.Giveaway)
        {
            this.GiveawayItem = giveawayItem;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
