using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayImageItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public override string FileType { get { return "image"; } set { } }

        public OverlayImageItemModel() : base() { }

        public OverlayImageItemModel(string filepath, int width, int height) : base(OverlayItemModelTypeEnum.Image, filepath, width, height) { }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
