using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum SAMMIActionTypeEnum
    {
        TriggerButton,
        ReleaseButton,
    }

    [DataContract]
    public class SAMMIActionModel : ActionModelBase
    {
        [DataMember]
        public SAMMIActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ButtonID { get; set; }

        public SAMMIActionModel(SAMMIActionTypeEnum actionType, string buttonID)
            : base(ActionTypeEnum.SAMMI)
        {
            this.ActionType = actionType;
            this.ButtonID = buttonID;
        }

        [Obsolete]
        public SAMMIActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<SAMMIService>().IsConnected)
            {
                string buttonID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.ButtonID, parameters);
                if (this.ActionType == SAMMIActionTypeEnum.TriggerButton)
                {
                    await ServiceManager.Get<SAMMIService>().TriggerButton(buttonID);
                }
                else if (this.ActionType == SAMMIActionTypeEnum.ReleaseButton)
                {
                    await ServiceManager.Get<SAMMIService>().ReleaseButton(buttonID);
                }
            }
        }
    }
}
