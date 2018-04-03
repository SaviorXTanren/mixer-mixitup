using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
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
    }

    [DataContract]
    public class DiscordAction : ActionBase
    {
        public static DiscordAction CreateForChatMessage(DiscordChannel channel, string message)
        {
            return new DiscordAction(DiscordActionTypeEnum.SendMessage) { SendMessageChannel = channel, SendMessageText = message };
        }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return DiscordAction.asyncSemaphore; } }

        [DataMember]
        public DiscordActionTypeEnum DiscordType { get; set; }

        [DataMember]
        public DiscordChannel SendMessageChannel { get; set; }
        [DataMember]
        public string SendMessageText { get; set; }

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
                    await ChannelSession.Services.Discord.CreateMessage(this.SendMessageChannel, message);
                }
            }
        }
    }
}
