using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum SAMMIActionTypeEnum
    {
        TriggerButton,
        ReleaseButton,
        SetGlobalVariable,
    }

    [DataContract]
    public class SAMMIActionModel : ActionModelBase
    {
        [DataMember]
        public SAMMIActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string ButtonID { get; set; }
        [DataMember]
        public Dictionary<string, string> ButtonVariables { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public string GlobalVariableName { get; set; }
        [DataMember]
        public string GlobalVariableValue { get; set; }

        public SAMMIActionModel(SAMMIActionTypeEnum actionType, string buttonID, Dictionary<string, string> buttonVariables)
            : base(ActionTypeEnum.SAMMI)
        {
            this.ActionType = actionType;
            this.ButtonID = buttonID;
            this.ButtonVariables = buttonVariables;
        }

        public SAMMIActionModel(SAMMIActionTypeEnum actionType, string globalVariableName, string globalVariableValue)
            : base(ActionTypeEnum.SAMMI)
        {
            this.ActionType = actionType;
            this.GlobalVariableName = globalVariableName;
            this.GlobalVariableValue = globalVariableValue;
        }

        [Obsolete]
        public SAMMIActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<SAMMIService>().IsEnabled && !ServiceManager.Get<SAMMIService>().IsConnected)
            {
                await ServiceManager.Get<SAMMIService>().Connect();
            }

            if (ServiceManager.Get<SAMMIService>().IsConnected)
            {
                if (this.ActionType == SAMMIActionTypeEnum.TriggerButton || this.ActionType == SAMMIActionTypeEnum.ReleaseButton)
                {
                    string buttonID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.ButtonID, parameters);

                    foreach (var kvp in this.ButtonVariables)
                    {
                        string key = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Key, parameters);
                        string value = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Value, parameters);
                        await ServiceManager.Get<SAMMIService>().SetVariable(key, value, buttonID);
                    }

                    if (this.ActionType == SAMMIActionTypeEnum.TriggerButton)
                    {
                        await ServiceManager.Get<SAMMIService>().TriggerButton(buttonID);
                    }
                    else if (this.ActionType == SAMMIActionTypeEnum.ReleaseButton)
                    {
                        await ServiceManager.Get<SAMMIService>().ReleaseButton(buttonID);
                    }
                }
                else if (this.ActionType == SAMMIActionTypeEnum.SetGlobalVariable)
                {
                    string globalVariableName = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.GlobalVariableName, parameters);
                    string globalVariableValue = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.GlobalVariableValue, parameters);

                    await ServiceManager.Get<SAMMIService>().SetVariable(globalVariableName, globalVariableValue);
                }
            }
        }
    }
}
