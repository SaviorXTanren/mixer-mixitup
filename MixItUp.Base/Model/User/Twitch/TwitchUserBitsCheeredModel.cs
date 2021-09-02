using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.Model.User.Twitch
{
    public class TwitchUserBitsCheeredModel
    {
        public UserV2ViewModel User { get; set; }

        public int Amount { get; set; }

        public TwitchChatMessageViewModel Message { get; set; }

        public TwitchUserBitsCheeredModel(UserV2ViewModel user, PubSubBitsEventV2Model bitsEvent)
        {
            this.User = user;
            this.Amount = bitsEvent.bits_used;
            this.Message = new TwitchChatMessageViewModel(user, bitsEvent);
        }
    }
}
