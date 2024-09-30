using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public class RandomActionModel : GroupActionModel
    {
        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public bool NoDuplicates { get; set; }

        [DataMember]
        public bool PersistNoDuplicates { get; set; }

        [JsonIgnore]
        public HashSet<Guid> triggered = new HashSet<Guid>();

        public RandomActionModel(string amount, bool noDuplicates, bool persistNoDuplicates, IEnumerable<ActionModelBase> actions)
            : base(ActionTypeEnum.Random, actions)
        {
            this.Amount = amount;
            this.NoDuplicates = noDuplicates;
            this.PersistNoDuplicates = persistNoDuplicates;
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

                List<ActionModelBase> actionsToUse = new List<ActionModelBase>(actionsToConsider);
                if (this.PersistNoDuplicates)
                {
                    actionsToUse.RemoveAll(a => this.triggered.Contains(a.ID));
                    if (actionsToUse.Count == 0)
                    {
                        this.triggered.Clear();
                        actionsToUse = new List<ActionModelBase>(actionsToConsider);
                    }
                }

                List<ActionModelBase> actionsToPerform = new List<ActionModelBase>();
                for (int i = 0; i < amount && actionsToUse.Count > 0; i++)
                {
                    ActionModelBase action = actionsToUse.Random();
                    if (this.NoDuplicates)
                    {
                        actionsToUse.Remove(action);
                        if (this.PersistNoDuplicates)
                        {
                            this.triggered.Add(action.ID);
                        }
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
