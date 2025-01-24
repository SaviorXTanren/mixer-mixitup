using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchBitsCheerViewModel : ChatEmoteViewModelBase
    {
        public static TwitchBitsCheerViewModel GetBitCheermote(string part)
        {
            foreach (var cheermote in ServiceManager.Get<TwitchSession>().BitsCheermotes.ToList())
            {
                if (part.StartsWith(cheermote.Key, StringComparison.InvariantCultureIgnoreCase) && int.TryParse(part.ToLower().Replace(cheermote.Key.ToLower(), ""), out int amount) && amount > 0)
                {
                    TwitchBitsCheermoteTierViewModel tier = cheermote.Value.GetAppropriateTier(amount);
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
