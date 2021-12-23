using Glimesh.Base.Models.Clients.Chat;
using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.ViewModel.Chat.Glimesh
{
    public class GlimeshChatMessageViewModel : UserChatMessageViewModel
    {
        public GlimeshChatMessageViewModel(ChatMessagePacketModel message, UserV2ViewModel user = null)
            : base(message.ID, StreamingPlatformTypeEnum.Glimesh, user)
        {
            foreach (ChatMessageTokenModel token in message.MessageTokens)
            {
                if (string.Equals(token.type, "text", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddStringMessagePart(token.text);
                }
                else if (string.Equals(token.type, "emote", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddStringMessagePart(token.text);
                    this.MessageParts[this.MessageParts.Count - 1] = new GlimeshChatEmoteViewModel(token);
                }
                else if (string.Equals(token.type, "url", StringComparison.OrdinalIgnoreCase))
                {
                    this.ContainsLink = true;
                    this.AddStringMessagePart(token.text);
                }
            }
        }
    }
}
