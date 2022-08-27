using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTextItemV3Model : OverlayVisualTextItemV3ModelBase
    {
        public static readonly string DefaultHTML = Resources.OverlayTextDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = string.Empty;

        public OverlayTextItemV3Model() : base(OverlayItemV3Type.Text) { }
    }
}
