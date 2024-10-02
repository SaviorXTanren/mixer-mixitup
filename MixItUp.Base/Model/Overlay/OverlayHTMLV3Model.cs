using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = "\n";
        public static readonly string DefaultCSS = "\n";
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript;

        public OverlayHTMLV3Model() : base(OverlayItemV3Type.HTML) { }

        public override Task Initialize()
        {
            if (string.Equals(this.Javascript, OverlayResources.OverlayHTMLWidgetDefaultJavascriptOld, System.StringComparison.OrdinalIgnoreCase))
            {
                this.Javascript = OverlayResources.OverlayHTMLWidgetDefaultJavascript;
            }
            return base.Initialize();
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.HTML)] = this.HTML;
            return properties;
        }
    }
}
