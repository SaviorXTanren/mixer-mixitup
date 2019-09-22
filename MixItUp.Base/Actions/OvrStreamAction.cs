using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum OvrStreamActionTypeEnum
    {
        [Obsolete]
        [Name("Show Title")]
        ShowTitle,
        [Name("Hide Title")]
        HideTitle,
        [Name("Play Title")]
        PlayTitle,
        [Name("Update Variables")]
        UpdateVariables,
        [Name("Enable Title")]
        EnableTitle,
        [Name("Disable Title")]
        DisableTitle,
    }

    [DataContract]
    public class OvrStreamAction : ActionBase
    {
        public static OvrStreamAction CreateVariableTitleAction(OvrStreamActionTypeEnum actionType, string titleName, Dictionary<string, string> variables)
        {
            return new OvrStreamAction(actionType)
            {
                TitleName = titleName,
                Variables = variables,
            };
        }

        public static OvrStreamAction CreateTitleAction(OvrStreamActionTypeEnum actionType, string titleName)
        {
            return new OvrStreamAction(actionType)
            {
                TitleName = titleName,
            };
        }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OvrStreamAction.asyncSemaphore; } }

        [DataMember]
        public OvrStreamActionTypeEnum OvrStreamActionType { get; set; }

        [DataMember]
        public string TitleName { get; set; }

        [DataMember]
        public Dictionary<string, string> Variables { get; set; }

        public OvrStreamAction()
            : base(ActionTypeEnum.OvrStream)
        {
        }

        public OvrStreamAction(OvrStreamActionTypeEnum ovrStreamActionType)
            : this()
        {
            this.OvrStreamActionType = ovrStreamActionType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.OvrStreamWebsocket != null)
            {
                if (this.OvrStreamActionType == OvrStreamActionTypeEnum.UpdateVariables ||
                    this.OvrStreamActionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    Dictionary<string, string> processedVariables = new Dictionary<string, string>();
                    foreach (var kvp in this.Variables)
                    {
                        processedVariables[kvp.Key] = await this.ReplaceStringWithSpecialModifiers(kvp.Value, user, arguments);

                        // Since OvrStream doesn't support URI based images, we need to trigger a download and get the path to those files
                        if (processedVariables[kvp.Key].StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string path = await ChannelSession.Services.OvrStreamWebsocket.DownloadImage(processedVariables[kvp.Key]);
                            if (path != null)
                            {
                                processedVariables[kvp.Key] = path;
                            }
                        }
                    }

                    switch (this.OvrStreamActionType)
                    {
                        case OvrStreamActionTypeEnum.UpdateVariables:
                            await ChannelSession.Services.OvrStreamWebsocket.UpdateVariables(this.TitleName, processedVariables);
                            break;
                        case OvrStreamActionTypeEnum.PlayTitle:
                            await ChannelSession.Services.OvrStreamWebsocket.PlayTitle(this.TitleName, processedVariables);
                            break;
                    }
                }
                else if (this.OvrStreamActionType == OvrStreamActionTypeEnum.HideTitle)
                {
                    await ChannelSession.Services.OvrStreamWebsocket.HideTitle(this.TitleName);
                }
                else if (this.OvrStreamActionType == OvrStreamActionTypeEnum.EnableTitle)
                {
                    await ChannelSession.Services.OvrStreamWebsocket.EnableTitle(this.TitleName);
                }
                else if (this.OvrStreamActionType == OvrStreamActionTypeEnum.DisableTitle)
                {
                    await ChannelSession.Services.OvrStreamWebsocket.DisableTitle(this.TitleName);
                }
            }
        }
    }
}
