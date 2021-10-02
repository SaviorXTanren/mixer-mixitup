using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum OvrStreamActionTypeEnum
    {
        PlayTitle,
        HideTitle,
        EnableTitle,
        DisableTitle,
        UpdateVariables,
    }

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

#pragma warning disable CS0612 // Type or member is obsolete
        internal OvrStreamActionModel(MixItUp.Base.Actions.OvrStreamAction action)
            : base(ActionTypeEnum.OvrStream)
        {
            switch (action.OvrStreamActionType)
            {
                case Base.Actions.OvrStreamActionTypeEnum.PlayTitle: this.ActionType = OvrStreamActionTypeEnum.PlayTitle; break;
                case Base.Actions.OvrStreamActionTypeEnum.HideTitle: this.ActionType = OvrStreamActionTypeEnum.HideTitle; break;
                case Base.Actions.OvrStreamActionTypeEnum.EnableTitle: this.ActionType = OvrStreamActionTypeEnum.EnableTitle; break;
                case Base.Actions.OvrStreamActionTypeEnum.DisableTitle: this.ActionType = OvrStreamActionTypeEnum.DisableTitle; break;
                case Base.Actions.OvrStreamActionTypeEnum.UpdateVariables: this.ActionType = OvrStreamActionTypeEnum.UpdateVariables; break;
            }
            this.TitleName = action.TitleName;
            this.Variables = action.Variables;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private OvrStreamActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.OvrStream.IsConnected)
            {
                if (this.ActionType == OvrStreamActionTypeEnum.UpdateVariables ||
                    this.ActionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    Dictionary<string, string> processedVariables = new Dictionary<string, string>();
                    foreach (var kvp in this.Variables)
                    {
                        processedVariables[kvp.Key] = await ReplaceStringWithSpecialModifiers(kvp.Value, parameters);

                        // Since OvrStream doesn't support URI based images, we need to trigger a download and get the path to those files
                        if (processedVariables[kvp.Key].StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string path = await ChannelSession.Services.OvrStream.DownloadImage(processedVariables[kvp.Key]);
                            if (path != null)
                            {
                                processedVariables[kvp.Key] = path;
                            }
                        }
                    }

                    switch (this.ActionType)
                    {
                        case OvrStreamActionTypeEnum.UpdateVariables:
                            await ChannelSession.Services.OvrStream.UpdateVariables(this.TitleName, processedVariables);
                            break;
                        case OvrStreamActionTypeEnum.PlayTitle:
                            await ChannelSession.Services.OvrStream.PlayTitle(this.TitleName, processedVariables);
                            break;
                    }
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.HideTitle)
                {
                    await ChannelSession.Services.OvrStream.HideTitle(this.TitleName);
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.EnableTitle)
                {
                    await ChannelSession.Services.OvrStream.EnableTitle(this.TitleName);
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.DisableTitle)
                {
                    await ChannelSession.Services.OvrStream.DisableTitle(this.TitleName);
                }
            }
        }
    }
}
