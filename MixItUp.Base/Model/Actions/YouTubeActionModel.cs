using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

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

        private YouTubeActionModel(YouTubeActionType type)
            : base(ActionTypeEnum.YouTube)
        {
            this.ActionType = type;
        }

        [Obsolete]
        public YouTubeActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<YouTubeSession>().IsConnected)
            {
                if (this.ActionType == YouTubeActionType.SetTitleDescription)
                {
                    string title = await ReplaceStringWithSpecialModifiers(this.Title, parameters);
                    string description = await ReplaceStringWithSpecialModifiers(this.Description, parameters);
                    Result result = await ServiceManager.Get<YouTubeSession>().UpdateStreamTitleAndDescription(title: title, description: description);
                    if (!result.Success)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(result.Message, parameters);
                    }
                }
                else if (this.ActionType == YouTubeActionType.RunAdBreak)
                {
                    foreach (LiveBroadcast broadcast in ServiceManager.Get<YouTubeSession>().LiveBroadcasts.Values.ToList())
                    {
                        LiveCuepoint response = await ServiceManager.Get<YouTubeSession>().StreamerService.StartAdBreak(broadcast, this.Amount);
                        if (response == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.YouTubeActionUnableToRunAdBreak, parameters);
                        }
                    }
                }
            }
        }
    }
}
