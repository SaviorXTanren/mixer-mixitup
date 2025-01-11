using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.Model.Twitch.Bits
{
    public class TwitchBitsCheeredEventModel
    {
        public UserV2ViewModel User { get; set; }

        public int Amount { get; set; }

        public TwitchChatMessageViewModel Message { get; set; }

        public TwitchBitsCheeredEventModel(UserV2ViewModel user, int amount, TwitchChatMessageViewModel message)
        {
            User = user;
            Amount = amount;
            Message = message;
        }
    }
}
