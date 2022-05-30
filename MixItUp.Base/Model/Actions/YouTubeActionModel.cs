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
        SetTitleDescription,
        RunAdBreak,
    }

    [DataContract]
    public class YouTubeActionModel : ActionModelBase
    {
        public static YouTubeActionModel CreateAdBreakAction(int amount)
        {
            YouTubeActionModel action = new YouTubeActionModel(YouTubeActionType.RunAdBreak);
            action.Amount = amount;
            return action;
        }

        public static YouTubeActionModel CreateSetTitleDescriptionAction(string title, string description)
        {
            YouTubeActionModel action = new YouTubeActionModel(YouTubeActionType.SetTitleDescription);
            action.Title = title;
            action.Description = description;
            return action;
        }

        [DataMember]
        public YouTubeActionType ActionType { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Amount { get; set; }

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
                if (this.ActionType == YouTubeActionType.SetTitleDescription)
                {
                    if (ServiceManager.Get<YouTubeSessionService>().IsLive && ServiceManager.Get<YouTubeSessionService>().Video != null)
                    {
                        await ServiceManager.Get<YouTubeSessionService>().UserConnection.UpdateVideo(ServiceManager.Get<YouTubeSessionService>().Video, title: this.Title, description: this.Description);
                    }
                }
                else if (this.ActionType == YouTubeActionType.RunAdBreak)
                {
                    if (ServiceManager.Get<YouTubeSessionService>().IsLive)
                    {
                        LiveCuepoint response = await ServiceManager.Get<YouTubeSessionService>().UserConnection.StartAdBreak(ServiceManager.Get<YouTubeSessionService>().Broadcast, this.Amount);
                        if (response == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.YouTubeActionUnableToRunAdBreak);
                        }
                    }
                }
            }
        }
    }
}
