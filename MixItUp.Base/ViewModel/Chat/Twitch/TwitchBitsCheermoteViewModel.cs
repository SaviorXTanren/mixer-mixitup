using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchBitsCheermoteViewModel
    {
        public BitsCheermoteModel Cheermote { get; set; }

        public Dictionary<string, TwitchBitsCheermoteTierViewModel> Tiers { get; set; } = new Dictionary<string, TwitchBitsCheermoteTierViewModel>();

        public TwitchBitsCheermoteViewModel(BitsCheermoteModel cheermote)
        {
            this.Cheermote = cheermote;
            foreach (BitsCheermoteTierModel tier in cheermote.tiers)
            {
                if (tier.can_cheer)
                {
                    this.Tiers[tier.id] = new TwitchBitsCheermoteTierViewModel(tier);
                }
            }
        }

        public string ID { get { return this.Cheermote.prefix; } }

        public TwitchBitsCheermoteTierViewModel GetAppropriateTier(int amount)
        {
            return this.Tiers.Where(t => t.Value.Amount <= amount).Top(t => t.Value.Amount).Value;
        }
    }

    public class TwitchBitsCheermoteTierViewModel
    {
        public BitsCheermoteTierModel Tier { get; set; }

        public TwitchBitsCheermoteTierViewModel(BitsCheermoteTierModel tier)
        {
            this.Tier = tier;
        }

        public string ID { get { return this.Tier.id; } }
        public int Amount { get { return this.Tier.min_bits; } }

        public string LightStaticImage { get { return (this.Tier.LightStaticImages.ContainsKey("2")) ? this.Tier.LightStaticImages["2"] : this.Tier.LightStaticImages.First().Value; } }
        public string DarkStaticImage { get { return (this.Tier.DarkStaticImages.ContainsKey("2")) ? this.Tier.DarkStaticImages["2"] : this.Tier.DarkStaticImages.First().Value; } }

        public string LightAnimatedImage { get { return (this.Tier.LightAnimatedImages.ContainsKey("2")) ? this.Tier.LightAnimatedImages["2"] : this.Tier.LightAnimatedImages.First().Value; } }
        public string DarkAnimatedImage { get { return (this.Tier.DarkAnimatedImages.ContainsKey("2")) ? this.Tier.DarkAnimatedImages["2"] : this.Tier.DarkAnimatedImages.First().Value; } }
    }
}
