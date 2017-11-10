using Mixer.Base.Clients;
using Mixer.Base.Model.Interactive;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Util
{
    public static class GlobalEvents
    {
        public static event EventHandler<UserViewModel> OnChatUserJoined;
        public static void ChatUserJoined(UserViewModel user)
        {
            if (GlobalEvents.OnChatUserJoined != null)
            {
                GlobalEvents.OnChatUserJoined(null, user);
            }
        }

        public static event EventHandler<ChatMessageViewModel> OnMessageReceived;
        public static void MessageReceived(ChatMessageViewModel message)
        {
            if (GlobalEvents.OnMessageReceived != null)
            {
                GlobalEvents.OnMessageReceived(null, message);
            }
        }

        public static event EventHandler<ChatMessageCommandViewModel> OnChatCommandMessageReceived;
        public static void ChatCommandMessageReceived(ChatMessageCommandViewModel chatCommandMessage)
        {
            if (GlobalEvents.OnChatCommandMessageReceived != null)
            {
                GlobalEvents.OnChatCommandMessageReceived(null, chatCommandMessage);
            }
        }

        public static event EventHandler<InteractiveControlModel> OnInteractiveControlUsed;
        public static void InteractiveControlUsed(InteractiveControlModel control)
        {
            if (GlobalEvents.OnInteractiveControlUsed != null)
            {
                GlobalEvents.OnInteractiveControlUsed(null, control);
            }
        }

        public static event EventHandler<ConstellationEventType> OnEventOccurred;
        public static void EventOccurred(ConstellationEventType eventType)
        {
            if (GlobalEvents.OnEventOccurred != null)
            {
                GlobalEvents.OnEventOccurred(null, eventType);
            }
        }

        public static event EventHandler<CommandBase> OnCommandExecuted;
        public static void CommandExecuted(CommandBase command)
        {
            if (GlobalEvents.OnCommandExecuted != null)
            {
                GlobalEvents.OnCommandExecuted(null, command);
            }
        }

        public static event EventHandler<string> OnQuoteAdded;
        public static void QuoteAdded(string quote)
        {
            if (GlobalEvents.OnQuoteAdded != null)
            {
                GlobalEvents.OnQuoteAdded(null, quote);
            }
        }

        public static event EventHandler OnGameQueueUpdated;
        public static void GameQueueUpdated()
        {
            if (GlobalEvents.OnGameQueueUpdated != null)
            {
                GlobalEvents.OnGameQueueUpdated(null, new EventArgs());
            }
        }

    }
}
