using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum DiscordActionTypeEnum
    {
        [Name("Send Message")]
        SendMessage,

        [Name("Mute/Unmute Self")]
        MuteSelf,
        [Name("Deafen/Undeafen Self")]
        DeafenSelf,
    }

    [DataContract]
    public class DiscordAction : ActionBase
    {
        public static DiscordAction CreateForChatMessage(DiscordChannel channel, string message, string filePath) { return new DiscordAction(DiscordActionTypeEnum.SendMessage) { SendMessageChannel = channel, SendMessageText = message, FilePath = filePath }; }

        public static DiscordAction CreateForMuteSelf(bool mute) { return new DiscordAction(DiscordActionTypeEnum.MuteSelf) { ShouldMuteDeafen = mute }; }

        public static DiscordAction CreateForDeafenSelf(bool deafen) { return new DiscordAction(DiscordActionTypeEnum.DeafenSelf) { ShouldMuteDeafen = deafen }; }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return DiscordAction.asyncSemaphore; } }

        [DataMember]
        public DiscordActionTypeEnum DiscordType { get; set; }

        [DataMember]
        public DiscordChannel SendMessageChannel { get; set; }
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Discord != null)
            {
                if (this.DiscordType == DiscordActionTypeEnum.SendMessage)
                {
                    string message = await this.ReplaceStringWithSpecialModifiers(this.SendMessageText, user, arguments);
                    await ChannelSession.Services.Discord.CreateMessage(this.SendMessageChannel, message, this.FilePath);
                }
                else if (this.DiscordType == DiscordActionTypeEnum.MuteSelf)
                {
                    await ChannelSession.Services.Discord.MuteServerMember(ChannelSession.Services.Discord.Server, ChannelSession.Services.Discord.User, this.ShouldMuteDeafen);
                }
                else if (this.DiscordType == DiscordActionTypeEnum.DeafenSelf)
                {
                    await ChannelSession.Services.Discord.DeafenServerMember(ChannelSession.Services.Discord.Server, ChannelSession.Services.Discord.User, this.ShouldMuteDeafen);
                }
            }
        }
    }
}
