using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayWebPageItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public override string FileType { get { return "webpage"; } set { } }

        public OverlayWebPageItemModel() : base() { }

        public OverlayWebPageItemModel(string filepath, int width, int height) : base(OverlayItemModelTypeEnum.WebPage, filepath, width, height) { }
    }
}
