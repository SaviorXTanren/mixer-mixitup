using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTextV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayTextDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript;

        public OverlayTextV3Model() : base(OverlayItemV3Type.Text) { }
    }
}
