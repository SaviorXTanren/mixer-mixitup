using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public class RepeatActionModel : GroupActionModel
    {
        [DataMember]
        public string Amount { get; set; }

        public RepeatActionModel(string amount, IEnumerable<ActionModelBase> actions)
            : base(ActionTypeEnum.Repeat, actions)
        {
            this.Amount = amount;
        }

        [Obsolete]
        public RepeatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string amountString = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Amount, parameters);
            if (int.TryParse(amountString, out int amount) && amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    await ServiceManager.Get<CommandService>().RunDirectly(new CommandInstanceModel(this.Actions, parameters));
                }
            }
        }
    }
}
