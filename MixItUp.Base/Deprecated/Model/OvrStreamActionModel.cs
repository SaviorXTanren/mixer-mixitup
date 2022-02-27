using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [Obsolete]
    public enum OvrStreamActionTypeEnum
    {
        PlayTitle,
        HideTitle,
        EnableTitle,
        DisableTitle,
        UpdateVariables,
    }

    [Obsolete]
    [DataContract]
    public class OvrStreamActionModel : ActionModelBase
    {
        [DataMember]
        public OvrStreamActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TitleName { get; set; }

        [DataMember]
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public OvrStreamActionModel(OvrStreamActionTypeEnum actionType, string titleName, Dictionary<string, string> variables = null)
            : base(ActionTypeEnum.OvrStream)
        {
            this.ActionType = actionType;
            this.TitleName = titleName;
            this.Variables = variables;
        }

        [Obsolete]
        public OvrStreamActionModel() { }

        protected override Task PerformInternal(CommandParametersModel parameters)
        {
            return Task.FromResult(0);
        }
    }
}
