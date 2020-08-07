using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum DiscordActionTypeEnum
    {
        SendMessage,
        MuteSelf,
        DeafenSelf,
    }

    [DataContract]
    public class DiscordActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return DiscordActionModel.asyncSemaphore; } }

        public static DiscordActionModel CreateForChatMessage(DiscordChannel channel, string message, string filePath) { return new DiscordActionModel(DiscordActionTypeEnum.SendMessage) { SendMessageChannelID = channel.ID, SendMessageText = message, FilePath = filePath }; }

        public static DiscordActionModel CreateForMuteSelf(bool mute) { return new DiscordActionModel(DiscordActionTypeEnum.MuteSelf) { ShouldMuteDeafen = mute }; }

        public static DiscordActionModel CreateForDeafenSelf(bool deafen) { return new DiscordActionModel(DiscordActionTypeEnum.DeafenSelf) { ShouldMuteDeafen = deafen }; }

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

        private DiscordChannel channel;

        public DiscordActionModel(DiscordActionTypeEnum type)
            : base(ActionTypeEnum.Discord)
        {
            this.DiscordType = type;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.DiscordType == DiscordActionTypeEnum.SendMessage)
            {
                if (this.channel == null)
                {
                    this.channel = await ChannelSession.Services.Discord.GetChannel(this.SendMessageChannelID);
                }

                if (this.channel != null)
                {
                    string message = await this.ReplaceStringWithSpecialModifiers(this.SendMessageText, user, platform, arguments, specialIdentifiers);
                    await ChannelSession.Services.Discord.CreateMessage(this.channel, message, this.FilePath);
                }
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
