using Twitch.Base.Models.NewAPI.Bits;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchBitsCheermoteViewModel
    {
        public BitsCheermoteModel Cheermote { get; set; }
        public BitsCheermoteTierModel Tier { get; set; }

        public TwitchBitsCheermoteViewModel(BitsCheermoteModel cheermote, BitsCheermoteTierModel tier)
        {
            this.Cheermote = cheermote;
            this.Tier = tier;
        }

        public string ID { get { return (!string.IsNullOrEmpty(this.Cheermote.prefix) && !string.IsNullOrEmpty(this.Tier.id)) ? this.Cheermote.prefix + this.Tier.id : string.Empty; } }

        public int Amount { get { return this.Tier.min_bits; } }

        public string LightImage { get { return (this.Tier.LightAnimatedImages.ContainsKey("2")) ? this.Tier.LightAnimatedImages["2"] : string.Empty; } }

        public string DarkImage { get { return (this.Tier.DarkAnimatedImages.ContainsKey("2")) ? this.Tier.DarkAnimatedImages["2"] : string.Empty; } }
    }
}
