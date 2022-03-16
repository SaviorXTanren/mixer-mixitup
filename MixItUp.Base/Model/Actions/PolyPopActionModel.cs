using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public class PolyPopActionModel : ActionModelBase
    {
        [DataMember]
        public string AlertName { get; set; }

        [DataMember]
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public PolyPopActionModel(string titleName, Dictionary<string, string> variables = null)
            : base(ActionTypeEnum.PolyPop)
        {
            this.AlertName = titleName;
            this.Variables = variables;
        }

        [Obsolete]
        public PolyPopActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<PolyPopService>().IsConnected)
            {
                Dictionary<string, string> processedVariables = new Dictionary<string, string>();
                foreach (var kvp in this.Variables)
                {
                    processedVariables[kvp.Key] = await ReplaceStringWithSpecialModifiers(kvp.Value, parameters);
                }

                await ServiceManager.Get<PolyPopService>().TriggerAlert(this.AlertName, processedVariables);
            }
        }
    }
}
