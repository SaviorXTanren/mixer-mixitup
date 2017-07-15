using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CooldownAction : ActionBase
    {
        public int CooldownAmount { get; set; }

        public CooldownAction(int cooldownAmount)
            : base(ActionTypeEnum.Cooldown)
        {
            this.CooldownAmount = cooldownAmount;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.CooldownAmount.ToString() }
            };
        }
    }
}
