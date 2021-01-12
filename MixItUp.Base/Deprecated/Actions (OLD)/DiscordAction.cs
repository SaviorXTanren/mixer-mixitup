using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum DiscordActionTypeEnum
    {
        SendMessage,
        MuteSelf,
        DeafenSelf,
    }

    [Obsolete]
    [DataContract]
    public class DiscordAction : ActionBase
    {
        public static DiscordAction CreateForChatMessage(DiscordChannel channel, string message, string filePath) { return new DiscordAction(DiscordActionTypeEnum.SendMessage) { SendMessageChannelID = channel.ID, SendMessageText = message, FilePath = filePath }; }

        public static DiscordAction CreateForMuteSelf(bool mute) { return new DiscordAction(DiscordActionTypeEnum.MuteSelf) { ShouldMuteDeafen = mute }; }

        public static DiscordAction CreateForDeafenSelf(bool deafen) { return new DiscordAction(DiscordActionTypeEnum.DeafenSelf) { ShouldMuteDeafen = deafen }; }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return DiscordAction.asyncSemaphore; } }

        [DataMember]
        public DiscordActionTypeEnum DiscordType { get; set; }

        [DataMember]
        public string SendMessageChannelID { get; set; }
        [DataMember]
        public string SendMessageText { get; set; }

        [DataMember]
        public bool ShouldMuteDeafen { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        public DiscordAction() : base(ActionTypeEnum.Discord) { }

        public DiscordAction(DiscordActionTypeEnum type)
            : this()
        {
            this.DiscordType = type;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}