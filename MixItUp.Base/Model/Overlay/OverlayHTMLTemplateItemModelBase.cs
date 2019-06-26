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

        public override async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            JObject jobj = await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers, encode);
            if (jobj != null)
            {
                string htmlTemplate = jobj["HTML"].ToString();
                foreach (var kvp in await this.GetTemplateReplacements(user, arguments, extraSpecialIdentifiers))
                {
                    htmlTemplate = htmlTemplate.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                jobj["HTML"] = htmlTemplate;
            }
            return jobj;
        }

        protected virtual Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
