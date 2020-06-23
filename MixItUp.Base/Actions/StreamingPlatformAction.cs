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
        public string PollQuestion { get; set; }
        [DataMember]
        public List<string> PollAnswers { get; set; }
        [DataMember]
        public uint PollLength { get; set; }
        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public string HostChannelName { get; set; }

        public CommandBase Command { get { return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); } }

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

        public static StreamingPlatformAction CreateRunAdAction()
        {
            return new StreamingPlatformAction(StreamingPlatformActionType.RunAd);
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
                AdResponseModel response = await ChannelSession.TwitchUserConnection.RunAd(ChannelSession.TwitchChannelNewAPI, 60);
                if (response == null)
                {
                    await ChannelSession.Services.Chat.Whisper(ChannelSession.GetCurrentUser(), "ERROR: We were unable to run an ad, please try again later");
                }
                else if (!string.IsNullOrEmpty(response.message))
                {
                    await ChannelSession.Services.Chat.Whisper(ChannelSession.GetCurrentUser(), "ERROR: " + response.message);
                }
            }
        }
    }
}
