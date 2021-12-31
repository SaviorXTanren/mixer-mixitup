using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum YouTubeActionType
    {
        RunAdBreak,
    }

    [DataContract]
    public class YouTubeActionModel : ActionModelBase
    {
        public static YouTubeActionModel CreateAdBreakAction(int adBreakLength)
        {
            YouTubeActionModel action = new YouTubeActionModel(YouTubeActionType.RunAdBreak);
            action.AdBreakLength = adBreakLength;
            return action;
        }

        [DataMember]
        public YouTubeActionType ActionType { get; set; }

        [DataMember]
        public int AdBreakLength { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        private YouTubeActionModel(YouTubeActionType type)
            : base(ActionTypeEnum.YouTube)
        {
            this.ActionType = type;
        }

        [Obsolete]
        public YouTubeActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                if (this.ActionType == YouTubeActionType.RunAdBreak)
                {
                    LiveCuepoint response = await ServiceManager.Get<YouTubeSessionService>().UserConnection.StartAdBreak(ServiceManager.Get<YouTubeChatService>()?.Broadcast, this.AdBreakLength);
                    if (response == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.YouTubeActionUnableToRunAdBreak);
                    }
                }
            }
        }
    }
}
