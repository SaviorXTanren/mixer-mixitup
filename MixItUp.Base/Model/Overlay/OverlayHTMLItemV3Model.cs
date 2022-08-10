using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHTMLItemV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;

        public OverlayHTMLItemV3Model() : base(OverlayItemV3Type.HTML) { }
    }
}
