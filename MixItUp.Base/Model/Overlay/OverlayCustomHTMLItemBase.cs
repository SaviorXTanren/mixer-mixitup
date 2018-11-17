using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayCustomHTMLItemBase : OverlayItemBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        public OverlayCustomHTMLItemBase() { }

        public OverlayCustomHTMLItemBase(string htmlTemplate)
        {
            this.HTMLText = htmlTemplate;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayCustomHTMLItemBase item = this.Copy<OverlayCustomHTMLItemBase>();
            foreach (var kvp in await this.GetReplacementSets(user, arguments, extraSpecialIdentifiers))
            {
                item.HTMLText = item.HTMLText.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            item.HTMLText = await this.ReplaceStringWithSpecialModifiers(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }

        protected virtual Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
