using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = "\n";
        public static readonly string DefaultCSS = "\n";
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript;

        public OverlayHTMLV3Model() : base(OverlayItemV3Type.HTML) { }
    }
}
