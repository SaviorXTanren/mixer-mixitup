using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTimer : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        public const string TimerItemType = "timer";

        [DataMember]
        public int TotalLength { get; set; }

        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        public OverlayTimer() : base(TimerItemType, HTMLTemplate) { }

        public OverlayTimer(string htmlText, int totalLength, string textColor, string textFont, int textSize)
            : base(TimerItemType, htmlText)
        {
            this.TotalLength = totalLength;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayTimer>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            return Task.FromResult(replacementSets);
        }
    }
}
