using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public class RandomActionModel : ActionModelBase
    {
        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public bool NoDuplicates { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public RandomActionModel(string amount, bool noDuplicates, IEnumerable<ActionModelBase> actions)
            : base(ActionTypeEnum.Random)
        {
            this.Amount = amount;
            this.NoDuplicates = noDuplicates;
            this.Actions = new List<ActionModelBase>(actions);
        }

        [Obsolete]
        public RandomActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string amountString = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Amount, parameters);
            if (int.TryParse(amountString, out int amount) && amount > 0)
            {
                List<ActionModelBase> actionsToConsider = new List<ActionModelBase>(this.Actions.Where(a => a.Enabled));

                List<ActionModelBase> actionsToRemove = new List<ActionModelBase>();
                foreach (ActionModelBase action in actionsToConsider)
                {
                    if (action.Type == ActionTypeEnum.Command)
                    {
                        CommandActionModel cAction = (CommandActionModel)action;
                        if (cAction != null)
                        {
                            if (cAction.ActionType == CommandActionTypeEnum.RunCommand)
                            {
                                CommandModelBase c = cAction.Command;
                                if (c != null && !c.IsEnabled)
                                {
                                    actionsToRemove.Add(action);
                                }
                            }
                        }
                    }
                }

                foreach (ActionModelBase action in actionsToRemove)
                {
                    actionsToConsider.Remove(action);
                }

                List<ActionModelBase> actionsToPerform = new List<ActionModelBase>();
                for (int i = 0; i < amount && actionsToConsider.Count > 0; i++)
                {
                    ActionModelBase action = actionsToConsider.Random();
                    if (this.NoDuplicates)
                    {
                        actionsToConsider.Remove(action);
                    }
                    actionsToPerform.Add(action);
                }

                if (actionsToPerform.Count > 0)
                {
                    await ServiceManager.Get<CommandService>().RunDirectly(new CommandInstanceModel(actionsToPerform, parameters));
                }
            }
        }
    }
}
