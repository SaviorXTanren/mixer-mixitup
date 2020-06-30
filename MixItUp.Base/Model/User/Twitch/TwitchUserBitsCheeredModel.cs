using MixItUp.Base.ViewModel.User;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.Model.User.Twitch
{
    public class TwitchUserBitsCheeredModel
    {
        public UserViewModel User { get; set; }

        public int Amount { get; set; }

        public string Message { get; set; }

        public TwitchUserBitsCheeredModel(UserViewModel user, PubSubBitsEventV2Model bitsEvent)
        {
            this.User = user;
            this.Amount = bitsEvent.bits_used;
            this.Message = !string.IsNullOrEmpty(bitsEvent.chat_message) ? bitsEvent.chat_message : string.Empty;
        }
    }
}
