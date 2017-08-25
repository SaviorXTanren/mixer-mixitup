using Mixer.Base.ViewModel;
using MixItUp.Base.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CurrencyAction : ActionBase
    {
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public CurrencyAction() { }

        public CurrencyAction(int amount, string chatText, bool isWhisper)
            : base(ActionTypeEnum.Currency)
        {
            this.Amount = amount;
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
