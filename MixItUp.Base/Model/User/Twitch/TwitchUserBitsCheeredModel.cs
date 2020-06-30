using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Bits;

namespace MixItUp.Base.Model.User.Twitch
{
    public class TwitchUserBitsCheeredModel
    {
        public UserViewModel User { get; set; }

        public int Amount { get; set; }

        public string Message { get; set; }

        public List<TwitchBitsCheermoteViewModel> BitsCheermotes { get; set; } = new List<TwitchBitsCheermoteViewModel>();

        public TwitchUserBitsCheeredModel(UserViewModel user, PubSubBitsEventV2Model bitsEvent)
        {
            this.User = user;
            this.Amount = bitsEvent.bits_used;
            this.Message = !string.IsNullOrEmpty(bitsEvent.chat_message) ? bitsEvent.chat_message : string.Empty;

            this.BitsCheermotes = new List<TwitchBitsCheermoteViewModel>(TwitchChatMessageViewModel.GetBitsCheermotesInMessage(this.Message));
            foreach (TwitchBitsCheermoteViewModel bitsCheermote in this.BitsCheermotes)
            {
                this.Message = this.Message.Replace(bitsCheermote.ID, "");
            }
            this.Message = this.Message.Replace("  ", " ");
        }
    }
}
