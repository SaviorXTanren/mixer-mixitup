using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayCustomHTMLItem : OverlayItemBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        [DataMember]
        public override string ItemType { get { return "custom"; } }

        public OverlayCustomHTMLItem() { }

        public OverlayCustomHTMLItem(string htmlTemplate)
        {
            this.HTMLText = htmlTemplate;
        }

        public virtual OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayCustomHTMLItem>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayCustomHTMLItem item = this.GetCopy();
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
