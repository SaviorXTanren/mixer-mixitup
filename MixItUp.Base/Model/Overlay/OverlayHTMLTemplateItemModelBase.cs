using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayHTMLTemplateItemModelBase : OverlayItemModelBase
    {
        [DataMember]
        public string HTML { get; set; }

        public OverlayHTMLTemplateItemModelBase() : base() { }

        public OverlayHTMLTemplateItemModelBase(OverlayItemModelTypeEnum type, string html)
            : base(type)
        {
            this.HTML = html;
        }

        protected override async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (jobj != null && jobj.ContainsKey("HTML"))
            {
                jobj["HTML"] = this.PerformTemplateReplacements(jobj["HTML"].ToString(), await this.GetTemplateReplacements(user, arguments, extraSpecialIdentifiers));
            }
            await base.PerformReplacements(jobj, user, arguments, extraSpecialIdentifiers);
        }

        protected string PerformTemplateReplacements(string text, Dictionary<string, string> templateReplacements)
        {
            foreach (var kvp in templateReplacements)
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return text;
        }

        protected virtual Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
