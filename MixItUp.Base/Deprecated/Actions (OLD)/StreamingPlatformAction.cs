using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum StreamingPlatformActionType
    {
        Host,
        [Obsolete]
        Poll,
        RunAd,
        Raid,
    }

    [Obsolete]
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

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
