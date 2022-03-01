using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
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

        [Obsolete]
        public OvrStreamActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<IOvrStreamService>().IsConnected)
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
                            string path = await ServiceManager.Get<IOvrStreamService>().DownloadImage(processedVariables[kvp.Key]);
                            if (path != null)
                            {
                                processedVariables[kvp.Key] = path;
                            }
                        }
                    }

                    switch (this.ActionType)
                    {
                        case OvrStreamActionTypeEnum.UpdateVariables:
                            await ServiceManager.Get<IOvrStreamService>().UpdateVariables(this.TitleName, processedVariables);
                            break;
                        case OvrStreamActionTypeEnum.PlayTitle:
                            await ServiceManager.Get<IOvrStreamService>().PlayTitle(this.TitleName, processedVariables);
                            break;
                    }
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.HideTitle)
                {
                    await ServiceManager.Get<IOvrStreamService>().HideTitle(this.TitleName);
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.EnableTitle)
                {
                    await ServiceManager.Get<IOvrStreamService>().EnableTitle(this.TitleName);
                }
                else if (this.ActionType == OvrStreamActionTypeEnum.DisableTitle)
                {
                    await ServiceManager.Get<IOvrStreamService>().DisableTitle(this.TitleName);
                }
            }
        }
    }
}