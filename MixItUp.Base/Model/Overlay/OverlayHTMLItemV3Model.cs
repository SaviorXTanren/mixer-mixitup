using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLItemV3Model : OverlayItemV3ModelBase
    {
        public const string DefaultHTML = "";

        public OverlayHTMLItemV3Model() : base(OverlayItemV3Type.HTML) { }
    }
}
