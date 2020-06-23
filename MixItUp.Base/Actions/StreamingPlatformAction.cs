using Mixer.Base.Model.Channel;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum StreamingPlatformActionType
    {
        Host,
        [Obsolete]
        Poll,
        [Name("Run Ad")]
        RunAd
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

        [JsonIgnore]
        private IEnumerable<string> lastArguments = null;

        public CommandBase Command { get { return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID)); } }

        public static StreamingPlatformAction CreateHostAction(string channelName)
        {
            StreamingPlatformAction action = new StreamingPlatformAction(StreamingPlatformActionType.Host);
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
            this.lastArguments = arguments;
            if (this.ActionType == StreamingPlatformActionType.Host)
            {
                string hostChannelName = await this.ReplaceStringWithSpecialModifiers(this.HostChannelName, user, arguments);
                ChannelModel channel = await ChannelSession.MixerUserConnection.GetChannel(hostChannelName);
                if (channel != null)
                {
                    await ChannelSession.MixerUserConnection.SetHostChannel(ChannelSession.MixerChannel, channel);
                }
            }
            else if (this.ActionType == StreamingPlatformActionType.RunAd)
            {
                bool result = await ChannelSession.MixerUserConnection.RunAd(ChannelSession.MixerChannel);
                if (!result)
                {
                    await ChannelSession.Services.Chat.Whisper(ChannelSession.GetCurrentUser(), "The ad could not be run, please verify your channel is approved for ads and that you have not already run an ad recently.");
                }
            }
        }
    }
}
