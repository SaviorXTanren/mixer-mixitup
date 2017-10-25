using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageCommandViewModel : ChatMessageViewModel
    {
        public static bool IsCommand(ChatMessageViewModel chatMessage)
        {
            return chatMessage.Message.StartsWith("!");
        }

        public string CommandName { get { return this.CommandPieces.First().Replace("!", "").ToLower(); } }

        public IEnumerable<string> CommandArguments { get { return this.CommandPieces.Skip(1); } }

        private IEnumerable<string> CommandPieces { get { return this.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); } }

        public ChatMessageCommandViewModel(ChatMessageViewModel chatMessage)
            : base(chatMessage.ChatMessageEvent)
        {
            if (!ChatMessageCommandViewModel.IsCommand(chatMessage))
            {
                throw new InvalidOperationException("Chat Message is not a command");
            }
        }
    }
}
