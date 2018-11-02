using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayHTMLItem : OverlayItemBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLItem() { }

        public OverlayHTMLItem(string htmlText)
        {
            this.HTMLText = htmlText;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayHTMLItem item = this.Copy<OverlayHTMLItem>();
            item.HTMLText = await this.ReplaceStringWithSpecialModifiers(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }
    }
}
