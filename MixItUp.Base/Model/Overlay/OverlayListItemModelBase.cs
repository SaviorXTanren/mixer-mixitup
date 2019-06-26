using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayListItemModelBase : OverlayHTMLTemplateItemModelBase
    {
        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayListItemModelBase() : base() { }

        public OverlayListItemModelBase(OverlayItemModelTypeEnum type, string htmlText, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(type, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.Effects = new OverlayItemEffectsModel(addEventAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, removeEventAnimation, 0);
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        protected override Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();

            return Task.FromResult(replacementSets);
        }
    }
}
