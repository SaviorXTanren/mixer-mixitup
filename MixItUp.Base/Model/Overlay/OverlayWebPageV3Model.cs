using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWebPageV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = Resources.OverlayWebPageDefaultHTML;
        public static readonly string DefaultCSS = string.Empty;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string URL { get; set; }

        public OverlayWebPageV3Model() : base(OverlayItemV3Type.WebPage) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.URL), this.URL);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.URL), this.URL);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.URL), this.URL);

            return item;
        }
    }
}