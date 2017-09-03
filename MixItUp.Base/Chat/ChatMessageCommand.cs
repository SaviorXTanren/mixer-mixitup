using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Chat
{
    public class ChatMessageCommand : ChatMessageViewModel
    {
        public static bool IsCommand(ChatMessageViewModel chatMessage)
        {
            return chatMessage.Message.StartsWith("!");
        }

        public string CommandName { get { return this.CommandPieces.First().Replace("!", ""); } }

        public IEnumerable<string> CommandArguments { get { return this.CommandPieces.Skip(1); } }

        private IEnumerable<string> CommandPieces { get { return this.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); } }

        public ChatMessageCommand(ChatMessageViewModel chatMessage)
            : base(chatMessage.ChatMessageEvent)
        {
            if (!ChatMessageCommand.IsCommand(chatMessage))
            {
                throw new InvalidOperationException("Chat Message is not a command");
            }
        }
    }
}
