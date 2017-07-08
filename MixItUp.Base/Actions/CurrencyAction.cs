using Mixer.Base.ViewModel;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CurrencyAction : ActionBase
    {
        public int Amount { get; set; }

        public CurrencyAction() : base("Currency") { }

        public override Task Perform(UserViewModel user)
        {
            throw new NotImplementedException();
        }
    }
}
