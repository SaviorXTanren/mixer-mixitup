using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum OvrStreamActionTypeEnum
    {
        [Name("Show Title")]
        ShowTitle,
        [Name("Hide Title")]
        HideTitle,
        [Name("Play Title")]
        PlayTitle,
    }


    [DataContract]
    public class OvrStreamAction : ActionBase
    {
        public static OvrStreamAction CreateTriggerTitleAction(OvrStreamActionTypeEnum actionType, string titleName, Dictionary<string, string> variables)
        {
            return new OvrStreamAction(actionType)
            {
                TitleName = titleName,
                Variables = variables,
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
                if (this.OvrStreamActionType == OvrStreamActionTypeEnum.ShowTitle ||
                    this.OvrStreamActionType == OvrStreamActionTypeEnum.HideTitle ||
                    this.OvrStreamActionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    Dictionary<string, string> processedVariables = new Dictionary<string, string>();
                    foreach (var kvp in this.Variables)
                    {
                        processedVariables[kvp.Key] = await this.ReplaceStringWithSpecialModifiers(kvp.Value, user, arguments);
                    }

                    switch (this.OvrStreamActionType)
                    {
                        case OvrStreamActionTypeEnum.ShowTitle:
                            await ChannelSession.Services.OvrStreamWebsocket.ShowTitle(this.TitleName, processedVariables);
                            break;
                        case OvrStreamActionTypeEnum.HideTitle:
                            await ChannelSession.Services.OvrStreamWebsocket.HideTitle(this.TitleName, processedVariables);
                            break;
                        case OvrStreamActionTypeEnum.PlayTitle:
                            await ChannelSession.Services.OvrStreamWebsocket.PlayTitle(this.TitleName, processedVariables);
                            break;
                    }
                }
            }
        }
    }
}
