using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlayHTMLItemModel : OverlayItemModelBase
    {
        [DataMember]
        public string HTML { get; set; }

        public OverlayHTMLItemModel() : base() { }

        public OverlayHTMLItemModel(string html)
            : base(OverlayItemModelTypeEnum.HTML)
        {
            this.HTML = html;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
