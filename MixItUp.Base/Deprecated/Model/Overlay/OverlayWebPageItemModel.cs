using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlayWebPageItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public override string FileType { get { return "webpage"; } set { } }

        public OverlayWebPageItemModel() : base() { }

        public OverlayWebPageItemModel(string filepath, int width, int height) : base(OverlayItemModelTypeEnum.WebPage, filepath, width, height) { }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
