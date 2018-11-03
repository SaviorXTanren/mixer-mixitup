using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWebPageItem : OverlayItemBase
    {
        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayWebPageItem() { }

        public OverlayWebPageItem(string url, int width, int height)
        {
            this.URL = url;
            this.Width = width;
            this.Height = height;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayWebPageItem item = this.Copy<OverlayWebPageItem>();
            item.URL = await this.ReplaceStringWithSpecialModifiers(item.URL, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }
}
