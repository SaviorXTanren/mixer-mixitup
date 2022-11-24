using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = string.Empty;
        public static readonly string DefaultCSS = string.Empty;
        public static readonly string DefaultJavascript = string.Empty;

        public OverlayHTMLV3Model() : base(OverlayItemV3Type.HTML) { }
    }
}
