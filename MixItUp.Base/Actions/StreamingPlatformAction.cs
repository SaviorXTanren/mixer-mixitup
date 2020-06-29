using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Ads;

namespace MixItUp.Base.Actions
{
    public enum StreamingPlatformActionType
    {
        Host,
        [Obsolete]
        Poll,
        [Name("Run Ad")]
        RunAd,
        Raid,
    }

    [DataContract]
    public class StreamingPlatformAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return StreamingPlatformAction.asyncSemaphore; } }

        [DataMember]
        public StreamingPlatformActionType ActionType { get; set; }

        [DataMember]
        public string HostChannelName { get; set; }

        [DataMember]
        public int AdLength { get; set; } = 60;

        public static StreamingPlatformAction CreateHostAction(string channelName)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.Host);
            action.HostChannelName = channelName;
            return action;
        }

        public static StreamingPlatformAction CreateRaidAction(string channelName)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.Raid);
            action.HostChannelName = channelName;
            return action;
        }

        public static StreamingPlatformAction CreateRunAdAction(int length)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.RunAd);
            action.AdLength = length;
            return action;
        }

        public StreamingPlatformAction() : base(ActionTypeEnum.StreamingPlatform) { }

        private StreamingPlatformAction(StreamingPlatformActionType type)
            : this()
        {
            this.ActionType = type;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.ActionType == StreamingPlatformActionType.Host)
            {
                string channelName = await this.ReplaceStringWithSpecialModifiers(this.HostChannelName, user, arguments);
                await ChannelSession.Services.Chat.SendMessage("/host @" + channelName, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
            }
            else if (this.ActionType == StreamingPlatformActionType.Raid)
            {
                string channelName = await this.ReplaceStringWithSpecialModifiers(this.HostChannelName, user, arguments);
                await ChannelSession.Services.Chat.SendMessage("/raid @" + channelName, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
            }
            else if (this.ActionType == StreamingPlatformActionType.RunAd)
            {
                AdResponseModel response = await ChannelSession.TwitchUserConnection.RunAd(ChannelSession.TwitchUserNewAPI, this.AdLength);
                if (response == null)
                {
                    await ChannelSession.Services.Chat.SendMessage("ERROR: We were unable to run an ad, please try again later");
                }
                else if (!string.IsNullOrEmpty(response.message))
                {
                    await ChannelSession.Services.Chat.SendMessage("ERROR: " + response.message);
                }
            }
        }
    }
}
