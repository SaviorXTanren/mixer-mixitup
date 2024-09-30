using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using System;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchBitsCheerViewModel : ChatEmoteViewModelBase
    {
        public static TwitchBitsCheerViewModel GetBitCheermote(string part)
        {
            foreach (TwitchBitsCheermoteViewModel cheermote in ServiceManager.Get<TwitchChatService>().BitsCheermotes)
            {
                if (part.StartsWith(cheermote.ID, StringComparison.InvariantCultureIgnoreCase) && int.TryParse(part.ToLower().Replace(cheermote.ID.ToLower(), ""), out int amount) && amount > 0)
                {
                    TwitchBitsCheermoteTierViewModel tier = cheermote.GetAppropriateTier(amount);
                    if (tier != null)
                    {
                        return new TwitchBitsCheerViewModel(part, amount, tier);
                    }
                }
            }
            return null;
        }

        public int Amount { get; set; }
        public TwitchBitsCheermoteTierViewModel Tier { get; set; }

        public TwitchBitsCheerViewModel(string text, int amount, TwitchBitsCheermoteTierViewModel tier)
        {
            this.Amount = amount;
            this.Tier = tier;

            this.ID = this.Name = text;

            this.ImageURL = ChannelSession.AppSettings.IsDarkBackground ? this.Tier.DarkStaticImage : this.Tier.LightStaticImage;
            this.AnimatedImageURL = ChannelSession.AppSettings.IsDarkBackground ? this.Tier.DarkAnimatedImage : this.Tier.LightAnimatedImage;
        }
    }
}
