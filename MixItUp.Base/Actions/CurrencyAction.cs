using Mixer.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CurrencyAction : ActionBase
    {
        public int Amount { get; set; }

        public CurrencyAction(int amount)
            : base(ActionTypeEnum.Currency)
        {
            this.Amount = amount;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.Amount.ToString() }
            };
        }
    }
}
