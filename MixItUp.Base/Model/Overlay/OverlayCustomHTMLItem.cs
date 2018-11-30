using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayCustomHTMLItem : OverlayItemBase
    {
        public const string CustomItemType = "custom";

        [DataMember]
        public string HTMLText { get; set; }

        public OverlayCustomHTMLItem() : base(OverlayCustomHTMLItem.CustomItemType) { }

        public OverlayCustomHTMLItem(string htmlTemplate) : this(OverlayCustomHTMLItem.CustomItemType, htmlTemplate) { }

        public OverlayCustomHTMLItem(string type, string htmlTemplate)
            : base(type)
        {
            this.HTMLText = htmlTemplate;
        }

        public virtual OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayCustomHTMLItem>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayCustomHTMLItem item = this.GetCopy();
            item.HTMLText = await this.PerformReplacement(item.HTMLText, user, arguments, extraSpecialIdentifiers);
            return item;
        }

        protected virtual async Task<string> PerformReplacement(string text, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            foreach (var kvp in await this.GetReplacementSets(user, arguments, extraSpecialIdentifiers))
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return await this.ReplaceStringWithSpecialModifiers(text, user, arguments, extraSpecialIdentifiers);
        }

        protected virtual Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
