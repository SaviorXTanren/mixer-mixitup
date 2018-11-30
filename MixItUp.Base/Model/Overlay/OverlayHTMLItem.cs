using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLItem : OverlayItemBase
    {
        public const string HTMLItemType = "html";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLItem() : base(HTMLItemType) { }

        public OverlayHTMLItem(string htmlText)
            : base(HTMLItemType)
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
