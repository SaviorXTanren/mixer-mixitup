using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
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

        protected override async Task PerformReplacements(JObject jobj, CommandParametersModel parameters)
        {
            if (jobj != null && jobj.ContainsKey("HTML"))
            {
                jobj["HTML"] = this.PerformTemplateReplacements(jobj["HTML"].ToString(), await this.GetTemplateReplacements(parameters));
            }
            await base.PerformReplacements(jobj, parameters);
        }

        protected string PerformTemplateReplacements(string text, Dictionary<string, string> templateReplacements)
        {
            foreach (var kvp in templateReplacements)
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return text;
        }

        protected virtual Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
